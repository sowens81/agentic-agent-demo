using Cloud.Infra.Orchestrator.Core.Agents;
using Cloud.Infra.Orchestrator.Core.Orchestration;
using Cloud.Infra.Orchestrator.Core.Records;
using Cloud.Infra.Orchestrator.Core.Services;
using Cloud.Infra.Orchestrator.Core.Services.Implementation;
using Cloud.Infra.Orchestrator.Infrastructure.Agents;
using Cloud.Infra.Orchestrator.Infrastructure.Dtos;
using Cloud.Infra.Orchestrator.Infrastructure.Persistence;
using Cloud.Infra.Orchestrator.Mappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

Console.WriteLine("========================================");
Console.WriteLine("Starting Cloud.Infra.Orchestrator");
Console.WriteLine("========================================");

var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");
Console.WriteLine($"OLLAMA_ENDPOINT = {endpoint}");

var agentsPath = Path.Combine(AppContext.BaseDirectory, "agents");
Console.WriteLine($"Loading agents from: {agentsPath}");

if (!Directory.Exists(agentsPath))
{
    Console.WriteLine("Agents directory not found. Exiting.");
    return;
}

var postgreSqlConnectionString =
    Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(postgreSqlConnectionString))
{
    Console.WriteLine("POSTGRESQL_CONNECTION_STRING not set. Exiting.");
    return;
}

// Try to apply SQL migrations if a migrations file is present in the repo.
try
{
    await ApplyMigrationsIfPresent(postgreSqlConnectionString);
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: failed to apply migrations: {ex.Message}");
}

/* ============================================================
 * HOST + DI
 * ============================================================ */

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging();

        services.AddScoped<
            IPersistencePostgreSql<AgentExecutionContextRecord>>(
            sp => new PersistencePostgreSql<AgentExecutionContextRecord>(
                sp.GetRequiredService<ILogger<PersistencePostgreSql<AgentExecutionContextRecord>>>(),
                postgreSqlConnectionString,
                "executions_tbl"));

        services.AddScoped<
            IPersistencePostgreSql<ExecutionStepRecord>>(
            sp => new PersistencePostgreSql<ExecutionStepRecord>(
                sp.GetRequiredService<ILogger<PersistencePostgreSql<ExecutionStepRecord>>>(),
                postgreSqlConnectionString,
                "execution_steps_tbl"));

        services.AddScoped<
            IPersistencePostgreSql<ApprovalRecord>>(
            sp => new PersistencePostgreSql<ApprovalRecord>(
                sp.GetRequiredService<ILogger<PersistencePostgreSql<ApprovalRecord>>>(),
                postgreSqlConnectionString,
                "approvals_tbl"));

        services.AddScoped<
            IAgentExecutionContextStoreService,
            AgentExecutionContextStoreService>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var sp = scope.ServiceProvider;

var executionStore =
    sp.GetRequiredService<IAgentExecutionContextStoreService>();

/* ============================================================
 * AGENT REGISTRY
 * ============================================================ */

var registry = new AgentRegistry();
var loader = new YamlAgentConfigLoader();

foreach (var agentDir in Directory.GetDirectories(agentsPath))
{
    var agentYaml = Path.Combine(agentDir, "agent.yaml");
    if (!File.Exists(agentYaml))
        continue;

    AgentConfigDto dto = loader.Load(agentYaml);
    var config = AgentConfigMapper.Map(dto);
    var kernel = AgentKernelFactory.Create(config.Name);

    registry.Register(config, kernel);
    Console.WriteLine($"Registered agent: {config.Name}");
}

Console.WriteLine($"Loaded {registry.All.Count} agents.");

Console.WriteLine("Creating orchestrator...");
var orchestrator = new Orchestrator(registry);

/* ============================================================
 * LOAD OR CREATE EXECUTION CONTEXT
 * ============================================================ */

var executionId = GetExecutionIdFromArgsOrEnv();

AgentExecutionContext context =
    await executionStore.LoadAsync(executionId)
    ?? new AgentExecutionContext
    {
        Id = executionId,
        Goal = "Design secure Azure infrastructure"
    };

Console.WriteLine($"Execution Id: {context.Id}");

/* ============================================================
 * WORKFLOW
 * ============================================================ */

var workflow = new Workflow
{
    Name = "secure-infra-workflow",
    Steps =
    [
        new InvokeCapabilityStep
        {
            Name = "plan-infrastructure",
            Capability = "infrastructure",
            Prompt = _ =>
                "Create a step-by-step plan for a secure Azure dev environment."
        },

        new ApprovalStep
        {
            Name = "approve-infra-plan",
            Summary = ctx =>
            {
                var plan = ctx.GetLastOutputForCapability("infrastructure");
                return $"Approve the following infrastructure plan:\n\n{plan}";
            }
        },

        new ConditionalStep
        {
            Name = "security-review-if-needed",
            Condition = ctx =>
            {
                var infra = ctx.GetLastOutputForCapability("infrastructure");
                return infra != null &&
                       infra.Contains("secure", StringComparison.OrdinalIgnoreCase);
            },
            IfTrue = new InvokeCapabilityStep
            {
                Name = "security-review",
                Capability = "security-review",
                Prompt = ctx =>
                {
                    var infra = ctx.GetLastOutputForCapability("infrastructure");
                    return
$"""
Review the following plan for security risks.
Provide individual risk assessments using:
threat: low | medium | high | critical

{infra}
""";
                }
            }
        },

        new ConditionalStep
        {
            Name = "security-approval-needed",
            Condition = ctx =>
            {
                var secReview = ctx.GetLastOutputForCapability("security-review");
                return secReview != null &&
                       (secReview.Contains("threat: high", StringComparison.OrdinalIgnoreCase)
                        || secReview.Contains("threat: critical", StringComparison.OrdinalIgnoreCase));
            },
            IfTrue = new ApprovalStep
            {
                Name = "approve-security-review",
                Summary = ctx =>
                {
                    var secReview = ctx.GetLastOutputForCapability("security-review");
                    return $"Approve the following security review:\n\n{secReview}";
                }
            }
        },

        new InvokeCapabilityStep
        {
            Name = "generate-docs",
            Capability = "documentation",
            Prompt = ctx =>
            {
                var infra = ctx.GetLastOutputForCapability("infrastructure");
                return $"Generate README.md documentation:\n\n{infra}";
            }
        }
    ]
};

/* ============================================================
 * EXECUTION LOOP (APPROVAL + PERSISTENCE)
 * ============================================================ */

while (true)
{
    try
    {
        await workflow.ExecuteAsync(context, orchestrator);
        await executionStore.SaveAsync(context);
        break;
    }
    catch (ApprovalRequiredException ex)
    {
        Console.WriteLine();
        Console.WriteLine("=================================");
        Console.WriteLine("APPROVAL REQUIRED");
        Console.WriteLine("=================================");
        Console.WriteLine(ex.Approval.Summary);
        Console.WriteLine();
        Console.Write("Approve? (y/n): ");

        var input = Console.ReadLine();

        if (input?.Equals("y", StringComparison.OrdinalIgnoreCase) == true)
        {
            ex.Approval.Approve("local-user");
            await executionStore.SaveAsync(context);
            Console.WriteLine("Approved. Resuming workflow...");
        }
        else
        {
            ex.Approval.Reject("local-user");
            await executionStore.SaveAsync(context);
            Console.WriteLine("Execution cancelled by user.");
            return;
        }
    }
}

/* ============================================================
 * OUTPUT
 * ============================================================ */

foreach (var step in context.Steps)
{
    Console.WriteLine($"[{step.ExecutedAt}] {step.AgentName} ({step.Capability})");
}

Console.WriteLine();
Console.WriteLine("========== APPROVALS ==========");

foreach (var approval in context.Approvals)
{
    Console.WriteLine(
        $"{approval.StepName} | Approved={approval.Approved} | By={approval.ApprovedBy} | At={approval.ApprovedAt}");
}

Console.WriteLine("Execution complete. Press ENTER to exit.");
Console.ReadLine();

static Guid GetExecutionIdFromArgsOrEnv()
{
    // 1️⃣ Command-line: --execution-id=<guid>
    var arg = Environment.GetCommandLineArgs()
        .FirstOrDefault(a => a.StartsWith("--execution-id=", StringComparison.OrdinalIgnoreCase));

    if (arg != null &&
        Guid.TryParse(arg.Split('=', 2)[1], out var parsed))
    {
        return parsed;
    }

    // 2️⃣ Environment variable
    var env = Environment.GetEnvironmentVariable("EXECUTION_ID");
    if (!string.IsNullOrWhiteSpace(env) &&
        Guid.TryParse(env, out parsed))
    {
        return parsed;
    }

    // 3️⃣ Default: create a NEW execution
    return Guid.NewGuid();
}

static async Task ApplyMigrationsIfPresent(string connectionString)
{
    // Look upward from the app base directory for 'databases/db_migrations.sql'
    var baseDir = AppContext.BaseDirectory;
    var dir = new DirectoryInfo(baseDir);

    FileInfo? migrationsFile = null;
    while (dir != null)
    {
        var candidate = Path.Combine(dir.FullName, "databases", "db_migrations.sql");
        if (File.Exists(candidate))
        {
            migrationsFile = new FileInfo(candidate);
            break;
        }

        dir = dir.Parent;
    }

    if (migrationsFile == null)
    {
        Console.WriteLine("No migrations file found (databases/db_migrations.sql). Skipping migrations.");
        return;
    }

    Console.WriteLine($"Applying migrations from: {migrationsFile.FullName}");
    var sql = await File.ReadAllTextAsync(migrationsFile.FullName);

    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    await cmd.ExecuteNonQueryAsync();

    Console.WriteLine("Migrations applied (if needed).");
}

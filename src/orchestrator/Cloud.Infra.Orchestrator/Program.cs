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

/* ============================================================
 * ENVIRONMENT CHECKS
 * ============================================================ */

var ollamaEndpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");
Console.WriteLine($"OLLAMA_ENDPOINT = {ollamaEndpoint}");

var agentsPath = Path.Combine(AppContext.BaseDirectory, "agents");
Console.WriteLine($"Loading agents from: {agentsPath}");

if (!Directory.Exists(agentsPath))
{
    Console.WriteLine("Agents directory not found. Exiting.");
    return;
}

var postgresConnectionString =
    Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(postgresConnectionString))
{
    Console.WriteLine("POSTGRESQL_CONNECTION_STRING not set. Exiting.");
    return;
}

/* ============================================================
 * HOST + DEPENDENCY INJECTION
 * ============================================================ */

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging();

        services.AddScoped<IPersistencePostgreSql<AgentExecutionContextRecord>>(sp =>
            new PersistencePostgreSql<AgentExecutionContextRecord>(
                sp.GetRequiredService<ILogger<PersistencePostgreSql<AgentExecutionContextRecord>>>(),
                postgresConnectionString,
                "executions_tbl"));

        services.AddScoped<IPersistencePostgreSql<ExecutionStepRecord>>(sp =>
            new PersistencePostgreSql<ExecutionStepRecord>(
                sp.GetRequiredService<ILogger<PersistencePostgreSql<ExecutionStepRecord>>>(),
                postgresConnectionString,
                "execution_steps_tbl"));

        services.AddScoped<IPersistencePostgreSql<ApprovalRecord>>(sp =>
            new PersistencePostgreSql<ApprovalRecord>(
                sp.GetRequiredService<ILogger<PersistencePostgreSql<ApprovalRecord>>>(),
                postgresConnectionString,
                "approvals_tbl"));

        services.AddScoped<IAgentExecutionContextStoreService, AgentExecutionContextStoreService>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var executionStore =
    services.GetRequiredService<IAgentExecutionContextStoreService>();

/* ============================================================
 * AGENT REGISTRY
 * ============================================================ */

var registry = new AgentRegistry();
var yamlLoader = new YamlAgentConfigLoader();

foreach (var agentDir in Directory.GetDirectories(agentsPath))
{
    var agentYaml = Path.Combine(agentDir, "agent.yaml");
    if (!File.Exists(agentYaml))
        continue;

    AgentConfigDto dto = yamlLoader.Load(agentYaml);
    var config = AgentConfigMapper.Map(dto);
    var kernel = AgentKernelFactory.Create(config.Name);

    registry.Register(config, kernel);
    Console.WriteLine($"Registered agent: {config.Name}");
}

Console.WriteLine($"Loaded {registry.All.Count} agents.");

/* ============================================================
 * ORCHESTRATOR
 * ============================================================ */

var orchestrator = new Orchestrator(registry);

/* ============================================================
 * LOAD OR CREATE EXECUTION CONTEXT
 * ============================================================ */

var executionId = GetExecutionIdFromArgsOrEnv();

var context =
    await executionStore.LoadAsync(executionId)
    ?? new AgentExecutionContext
    {
        Id = executionId,
        Goal = "Design secure Azure infrastructure"
    };

Console.WriteLine($"Execution Id: {context.Id}");
Console.WriteLine($"Goal: {context.Goal}");

/* ============================================================
 * PLANNING PHASE
 * ============================================================ */

Console.WriteLine("Creating execution plan...");
var plan = await orchestrator.CreatePlanAsync(context.Goal);

Console.WriteLine("Planner proposed steps:");
foreach (var step in plan.Steps)
{
    Console.WriteLine($"- {step.StepName} ({step.Capability})");
}

/* ============================================================
 * WORKFLOW CONSTRUCTION
 * ============================================================ */

var workflowSteps = plan.Steps
    .Select(step => new InvokeCapabilityStep
    {
        Name = step.StepName,
        Capability = step.Capability,
        Prompt = _ => step.Intent
    })
    .ToList();

var workflow = new Workflow
{
    Name = "planned-workflow",
    Steps = workflowSteps
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
 * FINAL OUTPUT
 * ============================================================ */

Console.WriteLine();
Console.WriteLine("Execution Steps:");
foreach (var step in context.Steps)
{
    Console.WriteLine($"[{step.ExecutedAt}] {step.AgentName} ({step.Capability})");
}

Console.WriteLine();
Console.WriteLine("Approvals:");
foreach (var approval in context.Approvals)
{
    Console.WriteLine(
        $"{approval.StepName} | Approved={approval.Approved} | By={approval.ApprovedBy} | At={approval.ApprovedAt}");
}

Console.WriteLine();
Console.WriteLine("Execution complete. Press ENTER to exit.");
Console.ReadLine();

/* ============================================================
 * HELPERS
 * ============================================================ */

static Guid GetExecutionIdFromArgsOrEnv()
{
    var arg = Environment.GetCommandLineArgs()
        .FirstOrDefault(a => a.StartsWith("--execution-id=", StringComparison.OrdinalIgnoreCase));

    if (arg != null &&
        Guid.TryParse(arg.Split('=', 2)[1], out var parsed))
    {
        return parsed;
    }

    var env = Environment.GetEnvironmentVariable("EXECUTION_ID");
    if (!string.IsNullOrWhiteSpace(env) &&
        Guid.TryParse(env, out parsed))
    {
        return parsed;
    }

    return Guid.NewGuid();
}

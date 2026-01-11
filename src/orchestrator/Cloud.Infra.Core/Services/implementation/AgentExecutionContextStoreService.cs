using Cloud.Infra.Orchestrator.Core.Orchestration;
using Cloud.Infra.Orchestrator.Core.Records;
using Cloud.Infra.Orchestrator.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Cloud.Infra.Orchestrator.Core.Services.Implementation;

public sealed class AgentExecutionContextStoreService
    : IAgentExecutionContextStoreService
{
    private readonly ILogger<AgentExecutionContextStoreService> _logger;
    private readonly IPersistencePostgreSql<AgentExecutionContextRecord> _contextRepo;
    private readonly IPersistencePostgreSql<ExecutionStepRecord> _stepRepo;
    private readonly IPersistencePostgreSql<ApprovalRecord> _approvalRepo;

    public AgentExecutionContextStoreService(
        ILogger<AgentExecutionContextStoreService> logger,
        IPersistencePostgreSql<AgentExecutionContextRecord> contextRepo,
        IPersistencePostgreSql<ExecutionStepRecord> stepRepo,
        IPersistencePostgreSql<ApprovalRecord> approvalRepo)
    {
        _logger = logger;
        _contextRepo = contextRepo;
        _stepRepo = stepRepo;
        _approvalRepo = approvalRepo;
    }

    // -------------------------
    // SAVE
    // -------------------------
    public async Task SaveAsync(AgentExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogDebug("Persisting AgentExecutionContext {ExecutionId}", context.Id);

        // 1️⃣ Upsert execution context (aggregate root)
        await _contextRepo.UpsertAsync(new AgentExecutionContextRecord
        {
            Id = context.Id,
            Goal = context.Goal,
            CurrentStepIndex = context.CurrentStepIndex,
            StartedAt = context.StartedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        // 2️⃣ Persist steps (deterministic identity: ExecutionId + StepIndex)
        foreach (var step in context.Steps)
        {
            await _stepRepo.UpsertAsync(new ExecutionStepRecord
            {
                ExecutionId = context.Id,
                StepIndex = step.StepIndex,
                Capability = step.Capability,
                AgentName = step.AgentName,
                InputData = step.InputData,
                OutputData = step.OutputData,
                ExecutedAt = step.ExecutedAt
            });
        }

        // 3️⃣ Persist approvals (deterministic identity: ExecutionId + StepName)
        foreach (var approval in context.Approvals)
        {
            await _approvalRepo.UpsertAsync(new ApprovalRecord
            {
                ExecutionId = context.Id,
                StepName = approval.StepName,
                Summary = approval.Summary,
                Approved = approval.Approved,
                ApprovedBy = approval.ApprovedBy,
                ApprovedAt = approval.ApprovedAt
            });
        }

        _logger.LogInformation(
            "AgentExecutionContext {ExecutionId} persisted successfully",
            context.Id);
    }

    // -------------------------
    // LOAD
    // -------------------------
    public async Task<AgentExecutionContext?> LoadAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ExecutionContext id cannot be empty", nameof(id));

        _logger.LogDebug("Loading AgentExecutionContext {ExecutionId}", id);

        var record = await _contextRepo.GetRecordAsync(id);
        if (record == null)
            return null;

        var context = new AgentExecutionContext
        {
            Id = record.Id,
            Goal = record.Goal,
            CurrentStepIndex = record.CurrentStepIndex,
            StartedAt = record.StartedAt
        };

        // 1️⃣ Rehydrate steps
        var steps = await _stepRepo.GetByExecutionIdAsync(id);
        foreach (var step in steps.OrderBy(s => s.StepIndex))
        {
            context.AddStep(new ExecutionStep
            {
                StepIndex = step.StepIndex,
                Capability = step.Capability,
                AgentName = step.AgentName,
                InputData = step.InputData,
                OutputData = step.OutputData,
                ExecutedAt = step.ExecutedAt
            });
        }

        // 2️⃣ Rehydrate approvals
        var approvals = await _approvalRepo.GetByExecutionIdAsync(id);
        foreach (var approval in approvals)
        {
            context.AddApproval(new Approval
            {
                StepName = approval.StepName,
                Summary = approval.Summary,
                Approved = approval.Approved,
                ApprovedBy = approval.ApprovedBy,
                ApprovedAt = approval.ApprovedAt
            });
        }

        return context;
    }
}

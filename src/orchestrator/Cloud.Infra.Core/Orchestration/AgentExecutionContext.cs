namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class AgentExecutionContext
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    public required string Goal { get; init; }

    private readonly List<ExecutionStep> _steps = [];
    private readonly List<Approval> _approvals = [];

    public IReadOnlyList<ExecutionStep> Steps => _steps;
    public IReadOnlyList<Approval> Approvals => _approvals;

    // ⭐ NEW
    public int CurrentStepIndex { get; set; } = 0;

    internal void AddStep(ExecutionStep step)
        => _steps.Add(step);

    internal void AddApproval(Approval approval)
        => _approvals.Add(approval);

    public Approval GetOrCreateApproval(
    string stepName,
    string summary)
    {
        var existing = _approvals.FirstOrDefault(a => a.StepName == stepName);
        if (existing != null)
            return existing;

        var approval = new Approval
        {
            StepName = stepName,
            Summary = summary
        };

        _approvals.Add(approval);
        return approval;
    }

    public Approval RequestApproval(string stepName, string summary)
    {
        var approval = new Approval
        {
            StepName = stepName,
            Summary = summary
        };

        _approvals.Add(approval);
        return approval;
    }

    public string? GetLastOutputForCapability(string capability)
    {
        return Steps
            .LastOrDefault(s => s.Capability == capability)
            ?.OutputData;
    }

}

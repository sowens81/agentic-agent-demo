namespace Cloud.Infra.Orchestrator.Core.Records;

public sealed class AgentExecutionContextRecord
{
    public Guid Id { get; set; }
    public string Goal { get; set; } = null!;

    public int CurrentStepIndex { get; set; }
    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

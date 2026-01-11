namespace Cloud.Infra.Orchestrator.Infrastructure.Records;

public sealed class ExecutionContextRecord
{
    public Guid Id { get; set; }
    public string Goal { get; set; } = null!;
    public int CurrentStepIndex { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

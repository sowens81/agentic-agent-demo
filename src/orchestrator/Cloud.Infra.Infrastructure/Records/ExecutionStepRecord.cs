namespace Cloud.Infra.Orchestrator.Infrastructure.Records;

public sealed class ExecutionStepRecord
{
    public Guid ExecutionId { get; set; }
    public int StepIndex { get; set; }
    public string Capability { get; set; } = null!;
    public string AgentName { get; set; } = null!;
    public string InputData { get; set; } = null!;
    public string OutputData { get; set; } = null!;
    public DateTimeOffset ExecutedAt { get; set; }
}


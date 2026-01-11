namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class ExecutionStep
{
    public required string Capability { get; init; }
    public required string AgentName { get; init; }
    public required string InputData { get; init; }
    public required string OutputData { get; init; }
    public required int StepIndex { get; init; }
    public DateTimeOffset ExecutedAt { get; init; } = DateTimeOffset.UtcNow;

}

namespace Cloud.Infra.Orchestrator.Core.Models;

public sealed class PlannerStep
{
    public required string StepName { get; init; }
    public required string Capability { get; init; }
    public required string Intent { get; init; }
}

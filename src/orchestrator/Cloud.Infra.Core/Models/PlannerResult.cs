namespace Cloud.Infra.Orchestrator.Core.Models;

public sealed class PlannerResult
{
    public required string Goal { get; init; }
    public required IReadOnlyList<PlannerStep> Steps { get; init; }
    public IReadOnlyList<string> Assumptions { get; init; } = [];
    public double Confidence { get; init; }
}

namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class Workflow
{
    public required string Name { get; init; }
    public required IReadOnlyList<WorkflowStep> Steps { get; init; }
    public async Task ExecuteAsync(
    AgentExecutionContext context,
    Orchestrator orchestrator)
    {
        while (context.CurrentStepIndex < Steps.Count)
        {
            var step = Steps[context.CurrentStepIndex];

            await step.ExecuteAsync(context, orchestrator);

            // Only advance if the step completed successfully
            context.CurrentStepIndex++;
        }
    }
}

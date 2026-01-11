namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class ConditionalStep : WorkflowStep
{
    public required Func<AgentExecutionContext, bool> Condition { get; init; }
    public required WorkflowStep IfTrue { get; init; }
    public WorkflowStep? IfFalse { get; init; }

    public override async Task ExecuteAsync(
        AgentExecutionContext context,
        Orchestrator orchestrator)
    {
        if (Condition(context))
        {
            await IfTrue.ExecuteAsync(context, orchestrator);
        }
        else if (IfFalse is not null)
        {
            await IfFalse.ExecuteAsync(context, orchestrator);
        }
    }
}

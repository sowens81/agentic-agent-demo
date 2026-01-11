namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class InvokeCapabilityStep : WorkflowStep
{
    public required string Capability { get; init; }
    public required Func<AgentExecutionContext, string> Prompt { get; init; }

    public override async Task ExecuteAsync(
        AgentExecutionContext context,
        Orchestrator orchestrator)
    {
        await orchestrator.InvokeByCapabilityAsync(
            context,
            Capability,
            Prompt(context));
    }
}

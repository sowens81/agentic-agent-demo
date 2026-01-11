namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public abstract class WorkflowStep
{
    public required string Name { get; init; }

    public abstract Task ExecuteAsync(
        AgentExecutionContext context,
        Orchestrator orchestrator);
}

namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class ApprovalStep : WorkflowStep
{
    public required Func<AgentExecutionContext, string> Summary { get; init; }

    public override Task ExecuteAsync(
    AgentExecutionContext context,
    Orchestrator orchestrator)
    {
        var approval = context.GetOrCreateApproval(
            Name,
            Summary(context));

        if (approval.Approved is null)
            throw new ApprovalRequiredException(approval);

        if (approval.Approved == false)
            throw new OperationCanceledException(
                $"Execution rejected at approval step '{Name}'.");

        // Approved == true → proceed
        return Task.CompletedTask;
    }

}

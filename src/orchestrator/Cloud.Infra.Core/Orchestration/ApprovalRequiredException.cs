namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class ApprovalRequiredException : Exception
{
    public Approval Approval { get; }

    public ApprovalRequiredException(Approval approval)
        : base($"Approval required for step '{approval.StepName}'.")
    {
        Approval = approval;
    }
}

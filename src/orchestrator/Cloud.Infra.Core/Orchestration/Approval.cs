namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class Approval
{
    public required string StepName { get; init; }
    public required string Summary { get; init; }

    public bool? Approved { get; internal set; }
    public string? ApprovedBy { get; internal set; }
    public DateTimeOffset? ApprovedAt { get; internal set; }

    public void Approve(string approvedBy)
    {
        Approved = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string approvedBy)
    {
        Approved = false;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTimeOffset.UtcNow;
    }
}

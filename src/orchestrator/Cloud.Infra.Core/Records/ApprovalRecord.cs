using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Infra.Orchestrator.Core.Records;

public sealed class ApprovalRecord
{
    public Guid ExecutionId { get; set; }
    public string StepName { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}
    
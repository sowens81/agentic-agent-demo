namespace Cloud.Infra.Orchestrator.Infrastructure.Dtos;

public sealed class AgentConfigDto
{
    public string ApiVersion { get; init; } = default!;
    public string Kind { get; init; } = default!;
    public MetadataDto Metadata { get; init; } = default!;
    public AgentSpecDto Spec { get; init; } = default!;
}

public sealed class MetadataDto
{
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
}

public sealed class AgentSpecDto
{
    public List<string> Capabilities { get; init; } = [];
    public List<string> Permissions { get; init; } = [];
    public ModelDto Model { get; init; } = default!;

    // 🔑 Dynamic behavior bag
    public Dictionary<string, object>? Behavior { get; init; }
}

public sealed class ModelDto
{
    public string Runtime { get; init; } = default!;
    public string Base { get; init; } = default!;
}


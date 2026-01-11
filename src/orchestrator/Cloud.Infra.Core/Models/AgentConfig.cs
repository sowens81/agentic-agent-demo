namespace Cloud.Infra.Orchestrator.Core.Models;

public sealed class AgentConfig
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<string> Capabilities { get; init; }
    public required ModelConfig Model { get; init; }

    public AgentBehavior Behavior { get; init; } =
        new AgentBehavior(new Dictionary<string, object>());
}


public sealed class ModelConfig
{
    public required string Runtime { get; init; }
    public required string Base { get; init; }
}

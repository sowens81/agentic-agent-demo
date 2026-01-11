using Cloud.Infra.Orchestrator.Core.Models;
using Microsoft.SemanticKernel;

namespace Cloud.Infra.Orchestrator.Core.Agents;

public class AgentDefinition
{
    public required AgentConfig Config { get; init; }
    public required Kernel Kernel { get; init; }
}

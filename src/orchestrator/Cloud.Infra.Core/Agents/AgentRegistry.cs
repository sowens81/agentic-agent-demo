using Cloud.Infra.Orchestrator.Core.Models;
using Microsoft.SemanticKernel;

namespace Cloud.Infra.Orchestrator.Core.Agents;

public sealed class AgentRegistry
{
    private readonly Dictionary<string, AgentDefinition> _agents = [];
    private readonly Dictionary<string, List<AgentDefinition>> _capabilityIndex = [];

    public void Register(AgentConfig config, Kernel kernel)
    {
        var agent = new AgentDefinition
        {
            Config = config,
            Kernel = kernel
        };

        _agents[config.Name] = agent;

        foreach (var capability in config.Capabilities)
        {
            if (!_capabilityIndex.TryGetValue(capability, out var list))
            {
                list = new List<AgentDefinition>();
                _capabilityIndex[capability] = list;
            }

            list.Add(agent);
        }

    }

    public IReadOnlyCollection<AgentDefinition> All => _agents.Values;

    public IReadOnlyList<AgentDefinition> GetByCapability(string capability)
    {
        return _capabilityIndex.TryGetValue(capability, out var agents)
            ? agents
            : [];
    }

    public AgentDefinition Get(string name)
    {
        if (!_agents.TryGetValue(name, out var agent))
        {
            throw new InvalidOperationException(
                $"Agent '{name}' is not registered.");
        }

        return agent;
    }
}

using Cloud.Infra.Orchestrator.Core.Agents;
using Microsoft.SemanticKernel;
using System;

namespace Cloud.Infra.Orchestrator.Core.Orchestration;

public sealed class Orchestrator
{
    private readonly AgentRegistry _registry;

    public Orchestrator(AgentRegistry registry)
    {
        _registry = registry;
    }

    public async Task<string> InvokeByCapabilityAsync(
        AgentExecutionContext context,
        string capability,
        string prompt)
    {
        var candidates = _registry.GetByCapability(capability);

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"No agents registered with capability '{capability}'.");
        }

        var agent = candidates[0];

        var output = await InvokeAgentAsync(agent, prompt);

        context.AddStep(new ExecutionStep
        {
            Capability = capability,
            AgentName = agent.Config.Name,
            StepIndex = context.Steps.Count,
            InputData = prompt,
            OutputData = output
        });

        return output;
    }

    private async Task<string> InvokeAgentAsync(
        AgentDefinition agent,
        string prompt)
    {
        var result = await agent.Kernel.InvokePromptAsync(prompt);
        return result.ToString();
    }

}

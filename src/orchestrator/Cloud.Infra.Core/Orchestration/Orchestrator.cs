using Cloud.Infra.Orchestrator.Core.Agents;
using Cloud.Infra.Orchestrator.Core.Models;
using Microsoft.SemanticKernel;
using System;
using System.Text.Json;

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

    public async Task<PlannerResult> CreatePlanAsync(string goal)
    {
        var planner = _registry.GetByCapability("planning").Single();

        var prompt =
        $"""
        Create an execution plan for the following goal:

        {goal}

        Respond ONLY with valid JSON.
        """;

        var raw = await planner.Kernel.InvokePromptAsync(prompt);

        var plan = JsonSerializer.Deserialize<PlannerResult>(
            raw.ToString(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? throw new InvalidOperationException("Invalid planner output");

        // ✅ VALIDATION LIVES HERE
        foreach (var step in plan.Steps)
        {
            if (!_registry.GetByCapability(step.Capability).Any())
            {
                throw new InvalidOperationException(
                    $"Planner requested unknown capability '{step.Capability}'");
            }
        }

        return plan;
    }
}

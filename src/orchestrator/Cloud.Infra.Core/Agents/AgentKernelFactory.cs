using Cloud.Infra.Orchestrator.Infrastructure.Ollama;
using Microsoft.SemanticKernel;

namespace Cloud.Infra.Orchestrator.Core.Agents;

public static class AgentKernelFactory
{
    public static Kernel Create(string modelName)
    {
        var options = OllamaClientOptions.FromEnvironment();

        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(
            modelId: modelName,
            apiKey: options.ApiKey,
            endpoint: options.Endpoint
        );

        return builder.Build();
    }
}

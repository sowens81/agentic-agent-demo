namespace Cloud.Infra.Orchestrator.Infrastructure.Ollama;

public sealed class OllamaClientOptions
{
    public required Uri Endpoint { get; init; }
    public required string ApiKey { get; init; }

    public static OllamaClientOptions FromEnvironment()
    {
        var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "OLLAMA_ENDPOINT environment variable is not set.");
        }

        var apiKey = Environment.GetEnvironmentVariable("OLLAMA_API_KEY") ?? "ollama";


        return new OllamaClientOptions
        {
            Endpoint = new Uri(endpoint),
            ApiKey = apiKey
        };
    }
}

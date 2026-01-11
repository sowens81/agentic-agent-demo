using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Cloud.Infra.Orchestrator.Infrastructure.Dtos;

namespace Cloud.Infra.Orchestrator.Infrastructure.Agents;

public sealed class YamlAgentConfigLoader
{
    private readonly IDeserializer _deserializer;

    public YamlAgentConfigLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public AgentConfigDto Load(string path)
    {
        var yaml = File.ReadAllText(path);
        return _deserializer.Deserialize<AgentConfigDto>(yaml);
    }
}

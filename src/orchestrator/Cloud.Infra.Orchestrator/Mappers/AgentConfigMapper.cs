using Cloud.Infra.Orchestrator.Core.Models;
using Cloud.Infra.Orchestrator.Infrastructure.Dtos;

namespace Cloud.Infra.Orchestrator.Mappers;
public static class AgentConfigMapper
{
    public static AgentConfig Map(AgentConfigDto dto)
    {
        ValidateContract(dto);

        return new AgentConfig
        {
            Name = dto.Metadata.Name,
            Description = dto.Metadata.Description,
            Capabilities = dto.Spec.Capabilities,

            Model = new ModelConfig
            {
                Runtime = dto.Spec.Model.Runtime,
                Base = dto.Spec.Model.Base
            },

            Behavior = new AgentBehavior(
                dto.Spec.Behavior ?? new Dictionary<string, object>())
        };
    }

    private static void ValidateContract(AgentConfigDto dto)
    {
        if (dto.ApiVersion != "agent.platform/v1")
            throw new InvalidOperationException($"Unsupported apiVersion {dto.ApiVersion}");

        if (dto.Kind != "Agent")
            throw new InvalidOperationException($"Unsupported kind {dto.Kind}");
    }
}


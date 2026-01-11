using Cloud.Infra.Orchestrator.Core.Orchestration;

namespace Cloud.Infra.Orchestrator.Core.Services;

public interface IAgentExecutionContextStoreService
{
    Task SaveAsync(AgentExecutionContext context);
    Task<AgentExecutionContext?> LoadAsync(Guid id);
}

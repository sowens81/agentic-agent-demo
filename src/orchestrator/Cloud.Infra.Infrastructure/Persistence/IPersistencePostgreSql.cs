namespace Cloud.Infra.Orchestrator.Infrastructure.Persistence;

public interface IPersistencePostgreSql<T> where T : class
{
    Task<T?> GetRecordAsync(Guid id);

    Task<IEnumerable<T>> GetByExecutionIdAsync(Guid executionId);

    Task InsertAsync(T record);

    Task UpsertAsync(T record);
}

using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Cloud.Infra.Orchestrator.Infrastructure.Persistence;

public sealed class PersistencePostgreSql<T>
    : IPersistencePostgreSql<T> where T : class
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly ILogger<PersistencePostgreSql<T>> _logger;

    public PersistencePostgreSql(
        ILogger<PersistencePostgreSql<T>> logger,
        string connectionString,
        string? tableName = null)
    {
        _logger = logger;
        _connectionString = connectionString;
        _tableName = tableName ?? typeof(T).Name;
    }

    public async Task<T?> GetRecordAsync(Guid id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        var sql = $"SELECT * FROM \"{_tableName}\" WHERE id = @Id LIMIT 1";
        return await conn.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<IEnumerable<T>> GetByExecutionIdAsync(Guid executionId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        var sql = $"SELECT * FROM \"{_tableName}\" WHERE executionid = @ExecutionId";
        return await conn.QueryAsync<T>(sql, new { ExecutionId = executionId });
    }

    public async Task InsertAsync(T record)
    {
        await ExecuteInsertAsync(record, allowConflict: false);
    }

    public async Task UpsertAsync(T record)
    {
        await ExecuteInsertAsync(record, allowConflict: true);
    }

    // -------------------------
    // INTERNAL
    // -------------------------
    private async Task ExecuteInsertAsync(T record, bool allowConflict)
    {
        var props = typeof(T).GetProperties()
            .Where(p => p.CanRead)
            .ToArray();

        var columns = string.Join(", ", props.Select(p => p.Name.ToLowerInvariant()));
        var values = string.Join(", ", props.Select(p => $"@{p.Name}"));

        var conflictClause = allowConflict
            ? "ON CONFLICT DO NOTHING"
            : string.Empty;

        var sql = $"""
            INSERT INTO "{_tableName}" ({columns})
            VALUES ({values})
            {conflictClause};
        """;

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(sql, record);
    }
}

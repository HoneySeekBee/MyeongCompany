using Dapper;
using MSMES.Domain.Common;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public class SqlAuditLogRepository : IAuditLogRepository
{
    private readonly ISqlConnectionFactory _db;
    public SqlAuditLogRepository(ISqlConnectionFactory db) => _db = db;

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        await conn.ExecuteAsync("""
            INSERT INTO AuditLogs (UserId, UserName, Action, EntityType, EntityId, Description, IpAddress, CreatedAt)
            VALUES (@UserId, @UserName, @Action, @EntityType, @EntityId, @Description, @IpAddress, @CreatedAt)
            """, log);
    }

    public async Task<IReadOnlyList<AuditLog>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync<AuditLog>(
            "SELECT * FROM AuditLogs ORDER BY CreatedAt DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY",
            new { skip, take });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<AuditLog>> ListByUserAsync(string userId, int take, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync<AuditLog>(
            "SELECT TOP (@take) * FROM AuditLogs WHERE UserId = @userId ORDER BY CreatedAt DESC",
            new { userId, take });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<AuditLog>> ListByEntityAsync(string entityType, string entityId, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync<AuditLog>(
            "SELECT * FROM AuditLogs WHERE EntityType = @entityType AND EntityId = @entityId ORDER BY CreatedAt DESC",
            new { entityType, entityId });
        return rows.ToList();
    }
}

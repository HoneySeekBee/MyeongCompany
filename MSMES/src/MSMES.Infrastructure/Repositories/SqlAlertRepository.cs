using Dapper;
using MSMES.Domain.Alert;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public class SqlAlertRepository : IAlertRepository
{
    private readonly ISqlConnectionFactory _db;
    public SqlAlertRepository(ISqlConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<Alert>> ListAsync(bool includeResolved, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var sql = includeResolved
            ? "SELECT * FROM Alerts ORDER BY Severity DESC, CreatedAt DESC"
            : "SELECT * FROM Alerts WHERE IsResolved = 0 ORDER BY Severity DESC, CreatedAt DESC";
        return (await conn.QueryAsync<Alert>(sql)).ToList();
    }

    public async Task<int> CountUnreadAsync(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Alerts WHERE IsRead = 0 AND IsResolved = 0");
    }

    public async Task MarkReadAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        await conn.ExecuteAsync("UPDATE Alerts SET IsRead = 1 WHERE Id = @id", new { id });
    }

    public async Task ResolveAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        await conn.ExecuteAsync("UPDATE Alerts SET IsResolved = 1, IsRead = 1 WHERE Id = @id", new { id });
    }
}

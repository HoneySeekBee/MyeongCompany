using Dapper;
using MSMES.Domain.Spc;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public class SqlSpcRepository : ISpcRepository
{
    private readonly ISqlConnectionFactory _db;
    public SqlSpcRepository(ISqlConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<SpcMeasurement>> GetByProcessAsync(string processCode, int subgroupCount, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync<SpcMeasurement>("""
            SELECT * FROM SpcMeasurements
            WHERE ProcessCode = @processCode
              AND SubgroupNo > (SELECT MAX(SubgroupNo) - @subgroupCount FROM SpcMeasurements WHERE ProcessCode = @processCode)
            ORDER BY SubgroupNo, SampleNo
            """, new { processCode, subgroupCount });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<string>> GetProcessListAsync(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync<string>(
            "SELECT DISTINCT ProcessCode + '|' + ProcessName + '|' + CharName FROM SpcMeasurements ORDER BY 1");
        return rows.ToList();
    }
}

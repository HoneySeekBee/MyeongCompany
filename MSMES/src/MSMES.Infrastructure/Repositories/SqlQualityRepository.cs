using Dapper;
using MSMES.Domain.Quality;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlQualityRepository : IQualityRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SqlQualityRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<QualityInspection?> GetByNoAsync(string inspectionNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = "SELECT * FROM dbo.QualityInspections WHERE InspectionNo = @inspectionNo";
        return await conn.QuerySingleOrDefaultAsync<QualityInspection>(
            new CommandDefinition(sql, new { inspectionNo }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<QualityInspection>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"SELECT * FROM dbo.QualityInspections ORDER BY InspectionDate DESC, InspectionNo DESC
                              OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
        var rows = await conn.QueryAsync<QualityInspection>(
            new CommandDefinition(sql, new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<QualityInspection>> ListByDateAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"SELECT * FROM dbo.QualityInspections
            WHERE InspectionDate >= @from AND InspectionDate <= @to
            ORDER BY InspectionDate DESC";
        var rows = await conn.QueryAsync<QualityInspection>(
            new CommandDefinition(sql, new { from, to }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(QualityInspection inspection, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.QualityInspections
            (InspectionNo, LotNo, InspectionItem, Result, InspectedQuantity, DefectQuantity,
             DefectTypeCode, InspectionDate, Inspector, Remarks, CreatedAt, CreatedBy)
            VALUES (@InspectionNo, @LotNo, @InspectionItem, @Result, @InspectedQuantity, @DefectQuantity,
                    @DefectTypeCode, @InspectionDate, @Inspector, @Remarks, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, inspection, cancellationToken: ct));
    }

    public async Task UpdateAsync(QualityInspection inspection, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.QualityInspections
            SET Result = @Result, DefectQuantity = @DefectQuantity, DefectTypeCode = @DefectTypeCode,
                Remarks = @Remarks, UpdatedAt = SYSUTCDATETIME()
            WHERE InspectionNo = @InspectionNo";
        await conn.ExecuteAsync(new CommandDefinition(sql, inspection, cancellationToken: ct));
    }

    public Task<string> NextNumberAsync(CancellationToken ct = default)
        => NumberSequence.NextAsync(_factory, "QI", "QI", ct);

    public async Task<IReadOnlyList<DefectType>> ListDefectTypesAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = "SELECT * FROM dbo.DefectTypes WHERE IsActive = 1 ORDER BY DefectTypeCode";
        var rows = await conn.QueryAsync<DefectType>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddDefectTypeAsync(DefectType type, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.DefectTypes
            (DefectTypeCode, DefectTypeName, DefectCause, IsActive, CreatedAt, CreatedBy)
            VALUES (@DefectTypeCode, @DefectTypeName, @DefectCause, @IsActive, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, type, cancellationToken: ct));
    }

    public async Task<(decimal TotalInspected, decimal TotalDefect, decimal DefectRate)> GetDefectStatsAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                ISNULL(SUM(InspectedQuantity), 0) AS TotalInspected,
                ISNULL(SUM(DefectQuantity), 0)    AS TotalDefect,
                CASE WHEN ISNULL(SUM(InspectedQuantity), 0) = 0
                     THEN 0
                     ELSE ROUND(SUM(DefectQuantity) / SUM(InspectedQuantity) * 100, 2)
                END AS DefectRate
            FROM dbo.QualityInspections
            WHERE InspectionDate >= @from AND InspectionDate <= @to";
        var row = await conn.QuerySingleAsync<(decimal TotalInspected, decimal TotalDefect, decimal DefectRate)>(
            new CommandDefinition(sql, new { from, to }, cancellationToken: ct));
        return row;
    }
}

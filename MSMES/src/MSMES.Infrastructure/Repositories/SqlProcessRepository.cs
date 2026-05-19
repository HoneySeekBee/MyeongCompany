using Dapper;
using MSMES.Domain.Process;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlProcessRepository : IProcessRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SqlProcessRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<ProcessDefinition?> GetProcessAsync(string processCode, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<ProcessDefinition>(new CommandDefinition(
            "SELECT * FROM dbo.ProcessDefinitions WHERE ProcessCode = @code",
            new { code = processCode }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<ProcessDefinition>> ListProcessesAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = "SELECT * FROM dbo.ProcessDefinitions WHERE IsActive = 1 ORDER BY ProcessOrder";
        var rows = await conn.QueryAsync<ProcessDefinition>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddProcessAsync(ProcessDefinition process, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            INSERT INTO dbo.ProcessDefinitions
                (ProcessCode, ProcessName, ProcessOrder, StandardTimeMinutes,
                 EquipmentType, IsActive, CreatedAt, CreatedBy)
            VALUES
                (@ProcessCode, @ProcessName, @ProcessOrder, @StandardTimeMinutes,
                 @EquipmentType, @IsActive, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, process, cancellationToken: ct));
    }

    public async Task UpdateProcessAsync(ProcessDefinition process, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            UPDATE dbo.ProcessDefinitions SET
                ProcessName         = @ProcessName,
                ProcessOrder        = @ProcessOrder,
                StandardTimeMinutes = @StandardTimeMinutes,
                EquipmentType       = @EquipmentType,
                IsActive            = @IsActive,
                UpdatedAt           = SYSUTCDATETIME()
            WHERE ProcessCode = @ProcessCode";
        await conn.ExecuteAsync(new CommandDefinition(sql, process, cancellationToken: ct));
    }

    public Task<string> NextResultNoAsync(CancellationToken ct = default)
        => NumberSequence.NextAsync(_factory, "PR", "PR", ct);

    public async Task AddResultAsync(ProductionResult result, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            INSERT INTO dbo.ProductionResults
                (ResultNo, WorkOrderNo, ProcessCode, Operator,
                 ProducedQuantity, DefectQuantity, StartTime, EndTime, CreatedAt, CreatedBy)
            VALUES
                (@ResultNo, @WorkOrderNo, @ProcessCode, @Operator,
                 @ProducedQuantity, @DefectQuantity, @StartTime, @EndTime, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, result, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<ProductionResult>> ListResultsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT * FROM dbo.ProductionResults
            WHERE StartTime >= @from AND StartTime < @to
            ORDER BY StartTime DESC";
        var rows = await conn.QueryAsync<ProductionResult>(new CommandDefinition(sql, new { from, to }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<ProductionResult>> ListResultsByWorkOrderAsync(string workOrderNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT * FROM dbo.ProductionResults
            WHERE WorkOrderNo = @no
            ORDER BY StartTime";
        var rows = await conn.QueryAsync<ProductionResult>(new CommandDefinition(sql, new { no = workOrderNo }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<(decimal Produced, decimal Defect)> GetDailyTotalsAsync(DateTime date, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                ISNULL(SUM(ProducedQuantity), 0) AS Produced,
                ISNULL(SUM(DefectQuantity),   0) AS Defect
            FROM dbo.ProductionResults
            WHERE CAST(StartTime AS DATE) = CAST(@date AS DATE)";
        var row = await conn.QuerySingleAsync(new CommandDefinition(sql, new { date }, cancellationToken: ct));
        return ((decimal)(row.Produced ?? 0m), (decimal)(row.Defect ?? 0m));
    }

    // 일별 생산 집계 (공정별)
    public async Task<IReadOnlyList<(DateTime Date, string ProcessCode, decimal Produced, decimal Defect)>> GetDailyProductionSummaryAsync(DateTime date, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                CAST(StartTime AS DATE)   AS Date,
                ProcessCode,
                SUM(ProducedQuantity)     AS Produced,
                SUM(DefectQuantity)       AS Defect
            FROM dbo.ProductionResults
            WHERE CAST(StartTime AS DATE) = CAST(@date AS DATE)
            GROUP BY CAST(StartTime AS DATE), ProcessCode
            ORDER BY ProcessCode";
        var rows = await conn.QueryAsync(new CommandDefinition(sql, new { date }, cancellationToken: ct));
        return rows.Select(r => ((DateTime)r.Date, (string)r.ProcessCode,
                                  (decimal)r.Produced, (decimal)r.Defect)).ToList();
    }
}

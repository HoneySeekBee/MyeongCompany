using Dapper;
using MSMES.Application.Dashboard;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlDashboardRepository : IDashboardRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SqlDashboardRepository(ISqlConnectionFactory factory) => _factory = factory;

    // 수주 잔고: Closed(4)/Cancelled 제외한 건수 + 수량 합계
    public async Task<(int Count, decimal Amount)> GetOpenSalesOrderBacklogAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                COUNT(*)                               AS Count,
                ISNULL(SUM(i.LineAmount), 0)           AS Amount
            FROM dbo.SalesOrders so
            LEFT JOIN (
                SELECT SalesOrderNo, SUM(Quantity * UnitPrice) AS LineAmount
                FROM dbo.SalesOrderItems
                GROUP BY SalesOrderNo
            ) i ON i.SalesOrderNo = so.SalesOrderNo
            WHERE so.Status NOT IN (3, 4)";
        var row = await conn.QuerySingleAsync(new CommandDefinition(sql, cancellationToken: ct));
        return ((int)(row.Count ?? 0), (decimal)(row.Amount ?? 0m));
    }

    // 작업지시 현황: Planned(0+1) / InProgress(2) / Completed(3+4)
    public async Task<(int Planned, int InProgress, int Completed)> GetWorkOrderStatusSummaryAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                SUM(CASE WHEN Status IN (0,1) THEN 1 ELSE 0 END) AS Planned,
                SUM(CASE WHEN Status = 2      THEN 1 ELSE 0 END) AS InProgress,
                SUM(CASE WHEN Status IN (3,4) THEN 1 ELSE 0 END) AS Completed
            FROM dbo.WorkOrders";
        var row = await conn.QuerySingleAsync(new CommandDefinition(sql, cancellationToken: ct));
        return ((int)(row.Planned ?? 0), (int)(row.InProgress ?? 0), (int)(row.Completed ?? 0));
    }

    // 일자별 생산수량 (라인차트용, 최근 7일)
    public async Task<IReadOnlyList<(DateTime Date, decimal Produced, decimal Defect)>> GetDailyProductionAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                CAST(StartTime AS DATE)         AS Date,
                SUM(ProducedQuantity)           AS Produced,
                SUM(DefectQuantity)             AS Defect
            FROM dbo.ProductionResults
            WHERE StartTime >= @from AND StartTime < DATEADD(DAY, 1, @to)
            GROUP BY CAST(StartTime AS DATE)
            ORDER BY CAST(StartTime AS DATE)";
        var rows = await conn.QueryAsync(new CommandDefinition(sql, new { from, to }, cancellationToken: ct));
        return rows.Select(r => ((DateTime)r.Date, (decimal)r.Produced, (decimal)r.Defect)).ToList();
    }

    // 최근 작업지시 N건
    public async Task<IReadOnlyList<(string WorkOrderNo, string ItemCode, decimal Quantity, string Status, DateTime CreatedAt)>> GetRecentWorkOrdersAsync(
        int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT TOP (@take)
                WorkOrderNo,
                ItemCode,
                PlannedQuantity AS Quantity,
                CAST(Status AS NVARCHAR(50)) AS Status,
                CreatedAt
            FROM dbo.WorkOrders
            ORDER BY CreatedAt DESC";
        var rows = await conn.QueryAsync(new CommandDefinition(sql, new { take }, cancellationToken: ct));
        return rows.Select(r => (
            (string)r.WorkOrderNo,
            (string)r.ItemCode,
            (decimal)r.Quantity,
            (string)r.Status,
            (DateTime)r.CreatedAt
        )).ToList();
    }
}

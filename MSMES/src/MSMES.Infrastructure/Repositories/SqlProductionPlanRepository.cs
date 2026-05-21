using Dapper;
using MSMES.Domain.ProductionPlan;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public class SqlProductionPlanRepository : IProductionPlanRepository
{
    private readonly ISqlConnectionFactory _db;
    public SqlProductionPlanRepository(ISqlConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<ProductionPlan>> ListAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync<ProductionPlan>(
            "SELECT * FROM ProductionPlans WHERE EndDate >= @from AND StartDate <= @to ORDER BY StartDate, Priority DESC",
            new { from = from.Date, to = to.Date });
        return rows.ToList();
    }

    public async Task<int> CreateAsync(ProductionPlan plan, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO ProductionPlans (PlanNo,WorkOrderNo,ProductCode,ProductName,PlannedQty,StartDate,EndDate,Line,Status,Priority,Remark,CreatedAt,CreatedBy)
            VALUES (@PlanNo,@WorkOrderNo,@ProductCode,@ProductName,@PlannedQty,@StartDate,@EndDate,@Line,@Status,@Priority,@Remark,@CreatedAt,@CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, plan);
    }

    public async Task UpdateStatusAsync(int id, byte status, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        await conn.ExecuteAsync("UPDATE ProductionPlans SET Status = @status WHERE Id = @id", new { id, status });
    }
}

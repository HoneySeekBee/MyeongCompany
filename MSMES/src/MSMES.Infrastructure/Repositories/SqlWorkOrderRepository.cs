using Dapper;
using MSMES.Domain.WorkOrder;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlWorkOrderRepository : IWorkOrderRepository
{
    private readonly ISqlConnectionFactory _factory;
    public SqlWorkOrderRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<WorkOrder?> GetByNoAsync(string workOrderNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<WorkOrder>(new CommandDefinition(
            "SELECT * FROM dbo.WorkOrders WHERE WorkOrderNo=@no", new { no = workOrderNo }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<WorkOrder>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<WorkOrder>(new CommandDefinition(
            "SELECT * FROM dbo.WorkOrders ORDER BY StartDate DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY",
            new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<WorkOrder>> ListBySalesOrderAsync(string salesOrderNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<WorkOrder>(new CommandDefinition(
            "SELECT * FROM dbo.WorkOrders WHERE SalesOrderNo=@so ORDER BY WorkOrderNo",
            new { so = salesOrderNo }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(WorkOrder w, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.WorkOrders
            (WorkOrderNo, SalesOrderNo, ItemCode, PlannedQuantity, ProducedQuantity, StartDate, PlannedEndDate, ActualEndDate, Status, CreatedAt, CreatedBy)
            VALUES (@WorkOrderNo,@SalesOrderNo,@ItemCode,@PlannedQuantity,@ProducedQuantity,@StartDate,@PlannedEndDate,@ActualEndDate,@Status,@CreatedAt,@CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, w, cancellationToken: ct));
    }

    public async Task UpdateAsync(WorkOrder w, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.WorkOrders SET SalesOrderNo=@SalesOrderNo, ItemCode=@ItemCode,
            PlannedQuantity=@PlannedQuantity, ProducedQuantity=@ProducedQuantity, StartDate=@StartDate,
            PlannedEndDate=@PlannedEndDate, ActualEndDate=@ActualEndDate, Status=@Status, UpdatedAt=SYSUTCDATETIME()
            WHERE WorkOrderNo=@WorkOrderNo";
        await conn.ExecuteAsync(new CommandDefinition(sql, w, cancellationToken: ct));
    }

    public Task<string> NextNumberAsync(CancellationToken ct = default) =>
        NumberSequence.NextAsync(_factory, "WO", "WO", ct);
}

using Dapper;
using MSMES.Domain.PurchaseOrder;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlPurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ISqlConnectionFactory _factory;
    public SqlPurchaseOrderRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<PurchaseOrder?> GetByNoAsync(string purchaseOrderNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var head = await conn.QuerySingleOrDefaultAsync<PurchaseOrder>(
            new CommandDefinition("SELECT * FROM dbo.PurchaseOrders WHERE PurchaseOrderNo=@no",
                new { no = purchaseOrderNo }, cancellationToken: ct));
        if (head is null) return null;
        var items = await conn.QueryAsync<PurchaseOrderItem>(
            new CommandDefinition("SELECT * FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo=@no ORDER BY ItemNo",
                new { no = purchaseOrderNo }, cancellationToken: ct));
        head.Items = items.ToList();
        return head;
    }

    public async Task<IReadOnlyList<PurchaseOrder>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<PurchaseOrder>(new CommandDefinition(
            "SELECT * FROM dbo.PurchaseOrders ORDER BY OrderDate DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY",
            new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(PurchaseOrder po, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        const string sql = @"INSERT INTO dbo.PurchaseOrders
            (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate, Status, CreatedAt, CreatedBy)
            VALUES (@PurchaseOrderNo,@SupplierCode,@SupplierName,@OrderDate,@DueDate,@Status,@CreatedAt,@CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, po, tx, cancellationToken: ct));
        const string sqlI = @"INSERT INTO dbo.PurchaseOrderItems
            (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, CreatedAt, CreatedBy)
            VALUES (@PurchaseOrderNo,@ItemNo,@ItemCode,@ItemName,@OrderQuantity,@UnitPrice,@CreatedAt,@CreatedBy)";
        foreach (var i in po.Items)
        {
            i.PurchaseOrderNo = po.PurchaseOrderNo;
            i.CreatedAt = po.CreatedAt;
            i.CreatedBy = po.CreatedBy;
            await conn.ExecuteAsync(new CommandDefinition(sqlI, i, tx, cancellationToken: ct));
        }
        tx.Commit();
    }

    public async Task UpdateAsync(PurchaseOrder po, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.PurchaseOrders SET SupplierCode=@SupplierCode, SupplierName=@SupplierName,
            OrderDate=@OrderDate, DueDate=@DueDate, Status=@Status, UpdatedAt=SYSUTCDATETIME()
            WHERE PurchaseOrderNo=@PurchaseOrderNo";
        await conn.ExecuteAsync(new CommandDefinition(sql, po, cancellationToken: ct));
    }

    public Task<string> NextNumberAsync(CancellationToken ct = default) =>
        NumberSequence.NextAsync(_factory, "PO", "PO", ct);
}

using Dapper;
using MSMES.Domain.SalesOrder;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlSalesOrderRepository : ISalesOrderRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SqlSalesOrderRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<SalesOrder?> GetByNoAsync(string salesOrderNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sqlHead = "SELECT * FROM dbo.SalesOrders WHERE SalesOrderNo = @no";
        const string sqlItems = "SELECT * FROM dbo.SalesOrderItems WHERE SalesOrderNo = @no ORDER BY ItemNo";

        var head = await conn.QuerySingleOrDefaultAsync<SalesOrder>(new CommandDefinition(sqlHead, new { no = salesOrderNo }, cancellationToken: ct));
        if (head is null) return null;
        var items = await conn.QueryAsync<SalesOrderItem>(new CommandDefinition(sqlItems, new { no = salesOrderNo }, cancellationToken: ct));
        head.Items = items.ToList();
        return head;
    }

    public async Task<IReadOnlyList<SalesOrder>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"SELECT * FROM dbo.SalesOrders ORDER BY OrderDate DESC, SalesOrderNo DESC
                              OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
        var rows = await conn.QueryAsync<SalesOrder>(new CommandDefinition(sql, new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(SalesOrder order, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        const string sqlHead = @"INSERT INTO dbo.SalesOrders
            (SalesOrderNo, CustomerCode, CustomerName, OrderDate, DueDate, Status, CreatedAt, CreatedBy)
            VALUES (@SalesOrderNo, @CustomerCode, @CustomerName, @OrderDate, @DueDate, @Status, @CreatedAt, @CreatedBy);";
        await conn.ExecuteAsync(new CommandDefinition(sqlHead, order, tx, cancellationToken: ct));

        const string sqlItem = @"INSERT INTO dbo.SalesOrderItems
            (SalesOrderNo, ItemNo, ItemCode, ItemName, Quantity, UnitPrice, CreatedAt, CreatedBy)
            VALUES (@SalesOrderNo, @ItemNo, @ItemCode, @ItemName, @Quantity, @UnitPrice, @CreatedAt, @CreatedBy);";
        foreach (var i in order.Items)
        {
            i.SalesOrderNo = order.SalesOrderNo;
            i.CreatedAt = order.CreatedAt;
            i.CreatedBy = order.CreatedBy;
            await conn.ExecuteAsync(new CommandDefinition(sqlItem, i, tx, cancellationToken: ct));
        }
        tx.Commit();
    }

    public async Task UpdateAsync(SalesOrder order, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.SalesOrders
            SET CustomerCode=@CustomerCode, CustomerName=@CustomerName, OrderDate=@OrderDate,
                DueDate=@DueDate, Status=@Status, UpdatedAt=SYSUTCDATETIME()
            WHERE SalesOrderNo=@SalesOrderNo";
        await conn.ExecuteAsync(new CommandDefinition(sql, order, cancellationToken: ct));
    }

    public Task<string> NextNumberAsync(CancellationToken ct = default)
        => NumberSequence.NextAsync(_factory, "SO", "SO", ct);
}

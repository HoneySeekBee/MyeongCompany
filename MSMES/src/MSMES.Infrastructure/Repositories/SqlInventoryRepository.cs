using Dapper;
using MSMES.Domain.Inventory;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlInventoryRepository : IInventoryRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SqlInventoryRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<Inventory?> GetAsync(string itemCode, string warehouseCode, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = "SELECT * FROM dbo.Inventories WHERE ItemCode = @itemCode AND WarehouseCode = @warehouseCode";
        return await conn.QuerySingleOrDefaultAsync<Inventory>(
            new CommandDefinition(sql, new { itemCode, warehouseCode }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Inventory>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"SELECT * FROM dbo.Inventories ORDER BY ItemCode ASC
                              OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
        var rows = await conn.QueryAsync<Inventory>(
            new CommandDefinition(sql, new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<Inventory>> ListByStatusAsync(InventoryStatus status, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        // 상태는 도메인 계산 필드이므로 SQL에서 직접 조건 매핑
        const string sqlNormal = "SELECT * FROM dbo.Inventories WHERE CurrentStock > SafetyStock ORDER BY ItemCode";
        const string sqlLow    = "SELECT * FROM dbo.Inventories WHERE CurrentStock > 0 AND CurrentStock <= SafetyStock ORDER BY ItemCode";
        const string sqlOut    = "SELECT * FROM dbo.Inventories WHERE CurrentStock <= 0 ORDER BY ItemCode";

        var sql = status switch
        {
            InventoryStatus.Normal     => sqlNormal,
            InventoryStatus.LowStock   => sqlLow,
            InventoryStatus.OutOfStock => sqlOut,
            _                          => sqlNormal
        };

        var rows = await conn.QueryAsync<Inventory>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(Inventory inventory, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.Inventories
            (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, Unit, CreatedAt, CreatedBy)
            VALUES (@ItemCode, @ItemName, @WarehouseCode, @CurrentStock, @SafetyStock, @Unit, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, inventory, cancellationToken: ct));
    }

    public async Task UpdateAsync(Inventory inventory, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.Inventories
            SET CurrentStock = @CurrentStock, UpdatedAt = SYSUTCDATETIME()
            WHERE ItemCode = @ItemCode AND WarehouseCode = @WarehouseCode";
        await conn.ExecuteAsync(new CommandDefinition(sql, inventory, cancellationToken: ct));
    }

    public async Task AddTransactionAsync(InventoryTransaction tx, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.InventoryTransactions
            (ItemCode, WarehouseCode, TransactionType, Quantity, ReferenceNo, Remarks, TransactionDate, CreatedBy, CreatedAt)
            VALUES (@ItemCode, @WarehouseCode, @TransactionType, @Quantity, @ReferenceNo, @Remarks, @TransactionDate, @CreatedBy, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        var id = await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql, tx, cancellationToken: ct));
        tx.TransactionId = id;
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetTransactionsAsync(
        string itemCode, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"SELECT * FROM dbo.InventoryTransactions
            WHERE ItemCode = @itemCode
              AND (@from IS NULL OR TransactionDate >= @from)
              AND (@to   IS NULL OR TransactionDate <= @to)
            ORDER BY TransactionDate DESC";
        var rows = await conn.QueryAsync<InventoryTransaction>(
            new CommandDefinition(sql, new { itemCode, from, to }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<(int Normal, int Low, int Out)> GetStatusSummaryAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                SUM(CASE WHEN CurrentStock > SafetyStock THEN 1 ELSE 0 END) AS Normal,
                SUM(CASE WHEN CurrentStock > 0 AND CurrentStock <= SafetyStock THEN 1 ELSE 0 END) AS Low,
                SUM(CASE WHEN CurrentStock <= 0 THEN 1 ELSE 0 END) AS [Out]
            FROM dbo.Inventories";
        var row = await conn.QuerySingleAsync<(int Normal, int Low, int Out)>(
            new CommandDefinition(sql, cancellationToken: ct));
        return row;
    }
}

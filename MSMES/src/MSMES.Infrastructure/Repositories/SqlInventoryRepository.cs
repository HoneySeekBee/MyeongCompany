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
            (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock,
             WarehouseLocation, Unit, CreatedAt, CreatedBy)
            VALUES (@ItemCode, @ItemName, @WarehouseCode, @CurrentStock, @SafetyStock, @MaxStock,
                    @WarehouseLocation, @Unit, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, inventory, cancellationToken: ct));
    }

    public async Task UpdateAsync(Inventory inventory, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.Inventories
            SET CurrentStock      = @CurrentStock,
                SafetyStock       = @SafetyStock,
                MaxStock          = @MaxStock,
                WarehouseLocation = @WarehouseLocation,
                UpdatedAt         = SYSUTCDATETIME()
            WHERE ItemCode = @ItemCode AND WarehouseCode = @WarehouseCode";
        await conn.ExecuteAsync(new CommandDefinition(sql, inventory, cancellationToken: ct));
    }

    public async Task AddTransactionAsync(InventoryTransaction tx, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.InventoryTransactions
            (ItemCode, ItemName, WarehouseCode, TransactionType, Quantity, StockBefore, StockAfter,
             ReferenceNo, Remarks, TransactionDate, CreatedBy, CreatedAt)
            VALUES (@ItemCode, @ItemName, @WarehouseCode, @TransactionType, @Quantity, @StockBefore, @StockAfter,
                    @ReferenceNo, @Remarks, @TransactionDate, @CreatedBy, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        var id = await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql, tx, cancellationToken: ct));
        tx.TransactionId = id;
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetTransactionsAsync(
        string itemCode, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT t.*, ISNULL(i.ItemName, t.ItemCode) AS ItemName
            FROM dbo.InventoryTransactions t
            LEFT JOIN dbo.Inventories i ON i.ItemCode = t.ItemCode AND i.WarehouseCode = t.WarehouseCode
            WHERE t.ItemCode = @itemCode
              AND (@from IS NULL OR t.TransactionDate >= @from)
              AND (@to   IS NULL OR t.TransactionDate <= @to)
            ORDER BY t.TransactionDate DESC";
        var rows = await conn.QueryAsync<InventoryTransaction>(
            new CommandDefinition(sql, new { itemCode, from, to }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetRecentTransactionsAsync(
        int count, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT TOP (@count) t.*, ISNULL(i.ItemName, t.ItemCode) AS ItemName
            FROM dbo.InventoryTransactions t
            LEFT JOIN dbo.Inventories i ON i.ItemCode = t.ItemCode AND i.WarehouseCode = t.WarehouseCode
            ORDER BY t.TransactionDate DESC";
        var rows = await conn.QueryAsync<InventoryTransaction>(
            new CommandDefinition(sql, new { count }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<InventoryTransaction>> ListTransactionsAsync(
        string? itemCode, int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT t.*
            FROM dbo.InventoryTransactions t
            WHERE (@itemCode IS NULL OR t.ItemCode = @itemCode)
            ORDER BY t.TransactionDate DESC
            OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
        var rows = await conn.QueryAsync<InventoryTransaction>(
            new CommandDefinition(sql, new { itemCode, skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AdjustStockAsync(
        string itemCode, string warehouseCode,
        decimal delta, InventoryTransactionType txType,
        string reason, string? reference, string createdBy,
        CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            // 1. 현재 재고 조회 (StockBefore)
            var inv = await conn.QuerySingleOrDefaultAsync<Inventory>(
                new CommandDefinition(
                    "SELECT * FROM dbo.Inventories WHERE ItemCode = @itemCode AND WarehouseCode = @warehouseCode",
                    new { itemCode, warehouseCode },
                    transaction: tran,
                    cancellationToken: ct))
                ?? throw new InvalidOperationException($"재고를 찾을 수 없습니다: {itemCode}/{warehouseCode}");

            var stockBefore = inv.CurrentStock;

            // 2. 새 재고 계산
            decimal stockAfter = txType switch
            {
                InventoryTransactionType.Receipt    => stockBefore + delta,
                InventoryTransactionType.Issue      => stockBefore - delta,
                InventoryTransactionType.Adjustment => delta,   // Adjustment: delta = 절대값
                _                                   => stockBefore + delta
            };

            if (txType == InventoryTransactionType.Issue && stockAfter < 0)
                throw new InvalidOperationException($"재고가 부족합니다. 현재재고: {stockBefore:N2}, 출고요청: {delta:N2}");

            // 3. Inventories 업데이트
            await conn.ExecuteAsync(
                new CommandDefinition(
                    "UPDATE dbo.Inventories SET CurrentStock = @stockAfter, UpdatedAt = SYSUTCDATETIME() WHERE ItemCode = @itemCode AND WarehouseCode = @warehouseCode",
                    new { stockAfter, itemCode, warehouseCode },
                    transaction: tran,
                    cancellationToken: ct));

            // 4. 실제 변화량 계산 (Adjustment의 경우 diff 기록)
            var actualDelta = txType == InventoryTransactionType.Adjustment
                ? stockAfter - stockBefore
                : (txType == InventoryTransactionType.Issue ? delta : delta);

            // 5. InventoryTransactions INSERT
            await conn.ExecuteAsync(
                new CommandDefinition(
                    @"INSERT INTO dbo.InventoryTransactions
                        (ItemCode, ItemName, WarehouseCode, TransactionType, Quantity,
                         StockBefore, StockAfter, ReferenceNo, Remarks,
                         TransactionDate, CreatedBy, CreatedAt)
                      VALUES
                        (@itemCode, @itemName, @warehouseCode, @txType, @actualDelta,
                         @stockBefore, @stockAfter, @reference, @reason,
                         SYSUTCDATETIME(), @createdBy, SYSUTCDATETIME())",
                    new
                    {
                        itemCode,
                        itemName = inv.ItemName,
                        warehouseCode,
                        txType = (int)txType,
                        actualDelta,
                        stockBefore,
                        stockAfter,
                        reference,
                        reason,
                        createdBy
                    },
                    transaction: tran,
                    cancellationToken: ct));

            tran.Commit();
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<(int TodayIn, int TodayOut, int TodayAdj)> GetTodayTransactionCountsAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                SUM(CASE WHEN TransactionType = 0 THEN 1 ELSE 0 END) AS TodayIn,
                SUM(CASE WHEN TransactionType = 1 THEN 1 ELSE 0 END) AS TodayOut,
                SUM(CASE WHEN TransactionType = 2 THEN 1 ELSE 0 END) AS TodayAdj
            FROM dbo.InventoryTransactions
            WHERE CAST(TransactionDate AS DATE) = CAST(SYSUTCDATETIME() AS DATE)";
        var row = await conn.QuerySingleAsync<(int TodayIn, int TodayOut, int TodayAdj)>(
            new CommandDefinition(sql, cancellationToken: ct));
        return row;
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

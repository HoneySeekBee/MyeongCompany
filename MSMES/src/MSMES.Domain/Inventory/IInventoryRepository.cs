namespace MSMES.Domain.Inventory;

public interface IInventoryRepository
{
    Task<Inventory?> GetAsync(string itemCode, string warehouseCode, CancellationToken ct = default);
    Task<IReadOnlyList<Inventory>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Inventory>> ListByStatusAsync(InventoryStatus status, CancellationToken ct = default);
    Task AddAsync(Inventory inventory, CancellationToken ct = default);
    Task UpdateAsync(Inventory inventory, CancellationToken ct = default);
    Task AddTransactionAsync(InventoryTransaction tx, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryTransaction>> GetTransactionsAsync(string itemCode, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryTransaction>> GetRecentTransactionsAsync(int count, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryTransaction>> ListTransactionsAsync(string? itemCode, int skip, int take, CancellationToken ct = default);
    Task AdjustStockAsync(string itemCode, string warehouseCode, decimal delta, InventoryTransactionType txType, string reason, string? reference, string createdBy, CancellationToken ct = default);
    Task<(int TodayIn, int TodayOut, int TodayAdj)> GetTodayTransactionCountsAsync(CancellationToken ct = default);
    Task<(int Normal, int Low, int Out)> GetStatusSummaryAsync(CancellationToken ct = default);
}

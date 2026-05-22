using MSMES.Domain.Inventory;

namespace MSMES.Application.Inventory;

public sealed record CreateInventoryTransactionCommand(
    string ItemCode,
    string WarehouseCode,
    InventoryTransactionType TransactionType,
    decimal Quantity,
    string? ReferenceNo,
    string? Remarks,
    string CreatedBy);

public sealed class CreateInventoryTransactionHandler
{
    private readonly IInventoryRepository _repo;
    public CreateInventoryTransactionHandler(IInventoryRepository repo) => _repo = repo;

    public async Task<long> HandleAsync(CreateInventoryTransactionCommand cmd, CancellationToken ct = default)
    {
        var inv = await _repo.GetAsync(cmd.ItemCode, cmd.WarehouseCode, ct)
                  ?? throw new InvalidOperationException($"Inventory not found: {cmd.ItemCode}/{cmd.WarehouseCode}");

        if (cmd.TransactionType == InventoryTransactionType.Issue && inv.CurrentStock < cmd.Quantity)
            throw new InvalidOperationException("Insufficient stock");

        var stockBefore = inv.CurrentStock;
        inv.ApplyTransaction(cmd.TransactionType, cmd.Quantity);
        await _repo.UpdateAsync(inv, ct);

        var tx = new InventoryTransaction
        {
            ItemCode        = cmd.ItemCode,
            ItemName        = inv.ItemName,
            WarehouseCode   = cmd.WarehouseCode,
            TransactionType = cmd.TransactionType,
            Quantity        = cmd.Quantity,
            StockBefore     = stockBefore,
            StockAfter      = inv.CurrentStock,
            ReferenceNo     = cmd.ReferenceNo,
            Remarks         = cmd.Remarks,
            TransactionDate = DateTime.UtcNow,
            CreatedBy       = cmd.CreatedBy,
            CreatedAt       = DateTime.UtcNow
        };
        await _repo.AddTransactionAsync(tx, ct);
        return tx.TransactionId;
    }
}

public sealed record GetInventoryStatusQuery(InventoryStatus? Status, int Skip = 0, int Take = 50);

public sealed class GetInventoryStatusHandler
{
    private readonly IInventoryRepository _repo;
    public GetInventoryStatusHandler(IInventoryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Domain.Inventory.Inventory>> HandleAsync(GetInventoryStatusQuery q, CancellationToken ct = default)
        => q.Status.HasValue
            ? _repo.ListByStatusAsync(q.Status.Value, ct)
            : _repo.ListAsync(q.Skip, q.Take, ct);
}

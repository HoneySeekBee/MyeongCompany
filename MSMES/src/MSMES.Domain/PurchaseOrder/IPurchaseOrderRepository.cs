namespace MSMES.Domain.PurchaseOrder;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByNoAsync(string purchaseOrderNo, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrder>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task AddAsync(PurchaseOrder po, CancellationToken ct = default);
    Task UpdateAsync(PurchaseOrder po, CancellationToken ct = default);
    Task<string> NextNumberAsync(CancellationToken ct = default);
}

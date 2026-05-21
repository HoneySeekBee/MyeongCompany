namespace MSMES.Domain.Receiving;

public interface IGoodsReceiptRepository
{
    Task<IReadOnlyList<GoodsReceipt>> ListAsync(CancellationToken ct = default);
    Task<GoodsReceipt?>              GetAsync(int id, CancellationToken ct = default);
    Task<int>                        CreateAsync(GoodsReceipt receipt, CancellationToken ct = default);
    Task                             UpdateStatusAsync(int id, byte status, string? inspector, CancellationToken ct = default);
    Task                             UpdateItemsAsync(int id, IList<GoodsReceiptItem> items, CancellationToken ct = default);
    Task<string>                     NextReceiptNoAsync(CancellationToken ct = default);
}

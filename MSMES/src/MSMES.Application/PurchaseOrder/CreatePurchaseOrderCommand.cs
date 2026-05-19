using MSMES.Domain.PurchaseOrder;

namespace MSMES.Application.PurchaseOrder;

public sealed record CreatePurchaseOrderItemDto(string ItemCode, string ItemName, decimal OrderQuantity, decimal UnitPrice);

public sealed record CreatePurchaseOrderCommand(
    string SupplierCode,
    string SupplierName,
    DateTime OrderDate,
    DateTime DueDate,
    IReadOnlyList<CreatePurchaseOrderItemDto> Items,
    string CreatedBy);

public sealed class CreatePurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _repo;
    public CreatePurchaseOrderHandler(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreatePurchaseOrderCommand cmd, CancellationToken ct = default)
    {
        var no = await _repo.NextNumberAsync(ct);
        var po = new Domain.PurchaseOrder.PurchaseOrder
        {
            PurchaseOrderNo = no,
            SupplierCode = cmd.SupplierCode,
            SupplierName = cmd.SupplierName,
            OrderDate = cmd.OrderDate,
            DueDate = cmd.DueDate,
            Status = PurchaseOrderStatus.Draft,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        int n = 1;
        foreach (var i in cmd.Items)
        {
            po.Items.Add(new PurchaseOrderItem
            {
                PurchaseOrderNo = no, ItemNo = n++,
                ItemCode = i.ItemCode, ItemName = i.ItemName,
                OrderQuantity = i.OrderQuantity, UnitPrice = i.UnitPrice
            });
        }
        await _repo.AddAsync(po, ct);
        return no;
    }
}

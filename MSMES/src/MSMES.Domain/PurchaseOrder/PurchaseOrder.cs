using MSMES.Domain.Shared;

namespace MSMES.Domain.PurchaseOrder;

public class PurchaseOrder : Entity
{
    public string PurchaseOrderNo { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public List<PurchaseOrderItem> Items { get; set; } = new();

    public decimal TotalAmount => Items.Sum(i => i.Amount);

    public void Issue()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft POs can be issued.");
        Status = PurchaseOrderStatus.Issued;
        UpdatedAt = DateTime.UtcNow;
    }
}

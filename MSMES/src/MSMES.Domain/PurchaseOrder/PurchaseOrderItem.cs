using MSMES.Domain.Shared;

namespace MSMES.Domain.PurchaseOrder;

public class PurchaseOrderItem : Entity
{
    public int ItemNo { get; set; }
    public string PurchaseOrderNo { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal OrderQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount => OrderQuantity * UnitPrice;
}

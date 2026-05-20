using MSMES.Domain.Shared;

namespace MSMES.Domain.Inventory;

public class InventoryTransaction : Entity
{
    public long TransactionId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public InventoryTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }
}

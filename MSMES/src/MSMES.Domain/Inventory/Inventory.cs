using MSMES.Domain.Shared;

namespace MSMES.Domain.Inventory;

public class Inventory : Entity
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal SafetyStock { get; set; }
    public string Unit { get; set; } = "EA";

    public InventoryStatus Status =>
        CurrentStock <= 0 ? InventoryStatus.OutOfStock :
        CurrentStock < SafetyStock ? InventoryStatus.LowStock :
        InventoryStatus.Normal;

    public void ApplyTransaction(InventoryTransactionType type, decimal quantity)
    {
        CurrentStock = type switch
        {
            InventoryTransactionType.Receipt => CurrentStock + quantity,
            InventoryTransactionType.Issue => CurrentStock - quantity,
            InventoryTransactionType.Adjustment => quantity,
            _ => CurrentStock
        };
        UpdatedAt = DateTime.UtcNow;
    }
}

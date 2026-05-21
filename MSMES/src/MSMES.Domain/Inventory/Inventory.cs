using MSMES.Domain.Shared;

namespace MSMES.Domain.Inventory;

public class Inventory : Entity
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal CurrentStock      { get; set; }
    public decimal SafetyStock       { get; set; }
    public decimal MaxStock          { get; set; }
    public string? WarehouseLocation { get; set; }
    public string  Unit              { get; set; } = "EA";

    // ── Status (existing) ───────────────────────────────────
    public InventoryStatus Status =>
        CurrentStock <= 0 ? InventoryStatus.OutOfStock :
        CurrentStock < SafetyStock ? InventoryStatus.LowStock :
        InventoryStatus.Normal;

    // ── Real-time shortage detection ────────────────────────
    /// <summary>재고가 안전재고 이하인 경우 (부족 경고)</summary>
    public bool IsLowStock => SafetyStock > 0 && CurrentStock <= SafetyStock;

    /// <summary>재고가 안전재고의 50% 이하인 경우 (위험)</summary>
    public bool IsCriticalStock => SafetyStock > 0 && CurrentStock <= SafetyStock * 0.5m;

    /// <summary>현재재고 / 최대재고 비율 (0–100). MaxStock이 0이면 100으로 간주.</summary>
    public decimal StockHealthPct =>
        MaxStock > 0
            ? Math.Min(100m, Math.Round(CurrentStock / MaxStock * 100m, 0))
            : 100m;

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

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
    /// <summary>트랜잭션 전 재고</summary>
    public decimal StockBefore { get; set; }
    /// <summary>트랜잭션 후 재고</summary>
    public decimal StockAfter { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }

    // ── 계산 프로퍼티 ─────────────────────────────────────────────
    /// <summary>트랜잭션 유형 한글 이름</summary>
    public string TypeName => TransactionType switch
    {
        InventoryTransactionType.Receipt    => "입고",
        InventoryTransactionType.Issue      => "출고",
        InventoryTransactionType.Adjustment => "조정",
        _                                   => "-"
    };

    /// <summary>Bootstrap badge 배경 CSS 클래스</summary>
    public string TypeCss => TransactionType switch
    {
        InventoryTransactionType.Receipt    => "bg-success",
        InventoryTransactionType.Issue      => "bg-danger",
        InventoryTransactionType.Adjustment => "bg-warning text-dark",
        _                                   => "bg-secondary"
    };

    /// <summary>수량 표시 부호 (+/-/±)</summary>
    public string QtySign => TransactionType switch
    {
        InventoryTransactionType.Receipt => "+",
        InventoryTransactionType.Issue   => "-",
        _                                => "±"
    };

    /// <summary>수량 색상 CSS 클래스</summary>
    public string QtyCss => TransactionType switch
    {
        InventoryTransactionType.Receipt    => "text-success fw-bold",
        InventoryTransactionType.Issue      => "text-danger fw-bold",
        InventoryTransactionType.Adjustment => "text-warning fw-bold",
        _                                   => "text-muted"
    };
}

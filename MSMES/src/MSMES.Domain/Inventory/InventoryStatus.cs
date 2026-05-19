namespace MSMES.Domain.Inventory;

public enum InventoryStatus
{
    Normal = 0,
    LowStock = 1,
    OutOfStock = 2
}

public enum InventoryTransactionType
{
    Receipt = 0,     // 입고
    Issue = 1,       // 출고
    Adjustment = 2   // 조정
}

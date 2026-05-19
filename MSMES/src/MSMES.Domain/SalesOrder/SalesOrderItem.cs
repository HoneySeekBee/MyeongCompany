using MSMES.Domain.Shared;

namespace MSMES.Domain.SalesOrder;

public class SalesOrderItem : Entity
{
    public int ItemNo { get; set; }
    public string SalesOrderNo { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount => Quantity * UnitPrice;
}

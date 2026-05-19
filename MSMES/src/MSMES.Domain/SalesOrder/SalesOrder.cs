using MSMES.Domain.Shared;

namespace MSMES.Domain.SalesOrder;

public class SalesOrder : Entity
{
    public string SalesOrderNo { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public List<SalesOrderItem> Items { get; set; } = new();

    public decimal TotalAmount => Items.Sum(i => i.Amount);

    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft orders can be confirmed.");
        if (Items.Count == 0)
            throw new InvalidOperationException("Cannot confirm order with no items.");
        Status = SalesOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkInProduction()
    {
        if (Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException("Order must be Confirmed.");
        Status = SalesOrderStatus.InProduction;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkShipped()
    {
        if (Status != SalesOrderStatus.InProduction)
            throw new InvalidOperationException("Order must be InProduction.");
        Status = SalesOrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = SalesOrderStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }
}

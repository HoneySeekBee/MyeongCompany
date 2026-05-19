using MSMES.Domain.Shared;

namespace MSMES.Domain.Shipment;

public class Shipment : Entity
{
    public string ShipmentNo { get; set; } = string.Empty;
    public string SalesOrderNo { get; set; } = string.Empty;
    public DateTime ShipmentDate { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;
    public List<ShipmentItem> Items { get; set; } = new();

    public void Ship()
    {
        if (Status != ShipmentStatus.Picking && Status != ShipmentStatus.Draft)
            throw new InvalidOperationException("Shipment must be Draft or Picking to ship.");
        Status = ShipmentStatus.Shipped;
        ShipmentDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

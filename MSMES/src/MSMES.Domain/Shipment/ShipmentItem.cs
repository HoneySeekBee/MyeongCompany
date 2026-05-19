using MSMES.Domain.Shared;

namespace MSMES.Domain.Shipment;

public class ShipmentItem : Entity
{
    public int ItemNo { get; set; }
    public string ShipmentNo { get; set; } = string.Empty;
    public string LotNo { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public decimal ShipQuantity { get; set; }
}

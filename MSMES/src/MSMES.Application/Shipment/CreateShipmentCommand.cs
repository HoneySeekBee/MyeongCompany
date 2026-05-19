using MSMES.Domain.Shipment;

namespace MSMES.Application.Shipment;

public sealed record CreateShipmentItemDto(string LotNo, string ItemCode, decimal ShipQuantity);

public sealed record CreateShipmentCommand(
    string SalesOrderNo,
    DateTime ShipmentDate,
    string DeliveryAddress,
    IReadOnlyList<CreateShipmentItemDto> Items,
    string CreatedBy);

public sealed class CreateShipmentHandler
{
    private readonly IShipmentRepository _repo;
    public CreateShipmentHandler(IShipmentRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreateShipmentCommand cmd, CancellationToken ct = default)
    {
        var no = await _repo.NextNumberAsync(ct);
        var s = new Domain.Shipment.Shipment
        {
            ShipmentNo = no,
            SalesOrderNo = cmd.SalesOrderNo,
            ShipmentDate = cmd.ShipmentDate,
            DeliveryAddress = cmd.DeliveryAddress,
            Status = ShipmentStatus.Draft,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        int n = 1;
        foreach (var i in cmd.Items)
        {
            s.Items.Add(new ShipmentItem
            {
                ShipmentNo = no, ItemNo = n++,
                LotNo = i.LotNo, ItemCode = i.ItemCode, ShipQuantity = i.ShipQuantity
            });
        }
        await _repo.AddAsync(s, ct);
        return no;
    }
}

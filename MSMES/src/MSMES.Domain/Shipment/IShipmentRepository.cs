namespace MSMES.Domain.Shipment;

public interface IShipmentRepository
{
    Task<Shipment?> GetByNoAsync(string shipmentNo, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task AddAsync(Shipment shipment, CancellationToken ct = default);
    Task UpdateAsync(Shipment shipment, CancellationToken ct = default);
    Task<string> NextNumberAsync(CancellationToken ct = default);
}

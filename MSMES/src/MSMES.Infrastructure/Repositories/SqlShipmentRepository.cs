using Dapper;
using MSMES.Domain.Shipment;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlShipmentRepository : IShipmentRepository
{
    private readonly ISqlConnectionFactory _factory;
    public SqlShipmentRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<Shipment?> GetByNoAsync(string shipmentNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var head = await conn.QuerySingleOrDefaultAsync<Shipment>(new CommandDefinition(
            "SELECT * FROM dbo.Shipments WHERE ShipmentNo=@no", new { no = shipmentNo }, cancellationToken: ct));
        if (head is null) return null;
        var items = await conn.QueryAsync<ShipmentItem>(new CommandDefinition(
            "SELECT * FROM dbo.ShipmentItems WHERE ShipmentNo=@no ORDER BY ItemNo",
            new { no = shipmentNo }, cancellationToken: ct));
        head.Items = items.ToList();
        return head;
    }

    public async Task<IReadOnlyList<Shipment>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<Shipment>(new CommandDefinition(
            "SELECT * FROM dbo.Shipments ORDER BY ShipmentDate DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY",
            new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<Shipment>> ListAllAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<Shipment>(new CommandDefinition(
            "SELECT * FROM dbo.Shipments ORDER BY ShipmentDate DESC",
            cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<ShipmentItem>> GetItemsByShipmentNoAsync(string shipmentNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<ShipmentItem>(new CommandDefinition(
            "SELECT * FROM dbo.ShipmentItems WHERE ShipmentNo=@no ORDER BY ItemNo",
            new { no = shipmentNo }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<ShipmentItem>> GetRecentItemsAsync(int count, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<ShipmentItem>(new CommandDefinition(
            @"SELECT TOP (@count) si.*
              FROM dbo.ShipmentItems si
              INNER JOIN dbo.Shipments s ON s.ShipmentNo = si.ShipmentNo
              ORDER BY s.ShipmentDate DESC, si.ShipmentNo DESC, si.ItemNo",
            new { count }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(Shipment s, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        const string sql = @"INSERT INTO dbo.Shipments
            (ShipmentNo, SalesOrderNo, ShipmentDate, DeliveryAddress, Status, CreatedAt, CreatedBy)
            VALUES (@ShipmentNo,@SalesOrderNo,@ShipmentDate,@DeliveryAddress,@Status,@CreatedAt,@CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, s, tx, cancellationToken: ct));
        const string sqlI = @"INSERT INTO dbo.ShipmentItems
            (ShipmentNo, ItemNo, LotNo, ItemCode, ShipQuantity, CreatedAt, CreatedBy)
            VALUES (@ShipmentNo,@ItemNo,@LotNo,@ItemCode,@ShipQuantity,@CreatedAt,@CreatedBy)";
        foreach (var i in s.Items)
        {
            i.ShipmentNo = s.ShipmentNo;
            i.CreatedAt = s.CreatedAt;
            i.CreatedBy = s.CreatedBy;
            await conn.ExecuteAsync(new CommandDefinition(sqlI, i, tx, cancellationToken: ct));
        }
        tx.Commit();
    }

    public async Task UpdateAsync(Shipment s, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.Shipments SET SalesOrderNo=@SalesOrderNo, ShipmentDate=@ShipmentDate,
            DeliveryAddress=@DeliveryAddress, Status=@Status, UpdatedAt=SYSUTCDATETIME() WHERE ShipmentNo=@ShipmentNo";
        await conn.ExecuteAsync(new CommandDefinition(sql, s, cancellationToken: ct));
    }

    public Task<string> NextNumberAsync(CancellationToken ct = default) =>
        NumberSequence.NextAsync(_factory, "SHP", "SHP", ct);
}

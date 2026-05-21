using Dapper;
using MSMES.Domain.Receiving;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public class SqlGoodsReceiptRepository : IGoodsReceiptRepository
{
    private readonly ISqlConnectionFactory _db;
    public SqlGoodsReceiptRepository(ISqlConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<GoodsReceipt>> ListAsync(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var receipts = (await conn.QueryAsync<GoodsReceipt>(
            "SELECT * FROM GoodsReceipts ORDER BY CreatedAt DESC")).ToList();
        if (receipts.Any())
        {
            var ids = receipts.Select(r => r.Id).ToArray();
            var items = await conn.QueryAsync<GoodsReceiptItem>(
                "SELECT * FROM GoodsReceiptItems WHERE GoodsReceiptId IN @ids", new { ids });
            var lookup = items.ToLookup(i => i.GoodsReceiptId);
            foreach (var r in receipts) r.Items = lookup[r.Id].ToList();
        }
        return receipts;
    }

    public async Task<GoodsReceipt?> GetAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var receipt = await conn.QueryFirstOrDefaultAsync<GoodsReceipt>(
            "SELECT * FROM GoodsReceipts WHERE Id = @id", new { id });
        if (receipt is null) return null;
        receipt.Items = (await conn.QueryAsync<GoodsReceiptItem>(
            "SELECT * FROM GoodsReceiptItems WHERE GoodsReceiptId = @id", new { id })).ToList();
        return receipt;
    }

    public async Task<int> CreateAsync(GoodsReceipt receipt, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO GoodsReceipts (ReceiptNo,PurchaseOrderNo,SupplierName,ReceiptDate,Status,CreatedAt,CreatedBy)
            VALUES (@ReceiptNo,@PurchaseOrderNo,@SupplierName,@ReceiptDate,@Status,@CreatedAt,@CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, receipt, tx);
        foreach (var item in receipt.Items)
        {
            item.GoodsReceiptId = id;
            await conn.ExecuteAsync("""
                INSERT INTO GoodsReceiptItems (GoodsReceiptId,MaterialCode,MaterialName,OrderedQty,ReceivedQty,Unit)
                VALUES (@GoodsReceiptId,@MaterialCode,@MaterialName,@OrderedQty,@ReceivedQty,@Unit)
                """, item, tx);
        }
        tx.Commit();
        return id;
    }

    public async Task UpdateStatusAsync(int id, byte status, string? inspector, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        await conn.ExecuteAsync("""
            UPDATE GoodsReceipts SET Status=@status, InspectorName=@inspector,
            InspectedAt = CASE WHEN @status >= 2 THEN GETUTCDATE() ELSE InspectedAt END
            WHERE Id=@id
            """, new { id, status, inspector });
    }

    public async Task UpdateItemsAsync(int id, IList<GoodsReceiptItem> items, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        foreach (var item in items)
        {
            await conn.ExecuteAsync("""
                UPDATE GoodsReceiptItems SET InspectedQty=@InspectedQty, AcceptedQty=@AcceptedQty,
                RejectedQty=@RejectedQty, DefectReason=@DefectReason WHERE Id=@Id
                """, item, tx);
        }
        tx.Commit();
    }

    public async Task<string> NextReceiptNoAsync(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM GoodsReceipts");
        return $"GR-{DateTime.Today:yyyy}-{(count + 1):D3}";
    }
}

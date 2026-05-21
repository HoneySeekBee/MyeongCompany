using Dapper;
using MSMES.Domain.Bom;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public class SqlBomRepository : IBomRepository
{
    private readonly ISqlConnectionFactory _db;
    public SqlBomRepository(ISqlConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<Bom>> ListAsync(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var boms = (await conn.QueryAsync<Bom>("SELECT * FROM Boms ORDER BY CreatedAt DESC")).ToList();
        if (boms.Any())
        {
            var ids = boms.Select(b => b.Id).ToArray();
            var items = await conn.QueryAsync<BomItem>("SELECT * FROM BomItems WHERE BomId IN @ids ORDER BY BomId, ItemNo", new { ids });
            var lookup = items.ToLookup(i => i.BomId);
            foreach (var bom in boms)
                bom.Items = lookup[bom.Id].ToList();
        }
        return boms;
    }

    public async Task<Bom?> GetAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var bom = await conn.QueryFirstOrDefaultAsync<Bom>("SELECT * FROM Boms WHERE Id = @id", new { id });
        if (bom is null) return null;
        bom.Items = (await conn.QueryAsync<BomItem>("SELECT * FROM BomItems WHERE BomId = @id ORDER BY ItemNo", new { id })).ToList();
        return bom;
    }

    public async Task<int> CreateAsync(Bom bom, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var bomId = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO Boms (BomNo,ProductCode,ProductName,Version,IsActive,Remark,CreatedAt,CreatedBy)
            VALUES (@BomNo,@ProductCode,@ProductName,@Version,@IsActive,@Remark,@CreatedAt,@CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, bom, tx);
        foreach (var item in bom.Items)
        {
            item.BomId = bomId;
            await conn.ExecuteAsync("""
                INSERT INTO BomItems (BomId,ItemNo,MaterialCode,MaterialName,Quantity,Unit,Remark)
                VALUES (@BomId,@ItemNo,@MaterialCode,@MaterialName,@Quantity,@Unit,@Remark)
                """, item, tx);
        }
        tx.Commit();
        return bomId;
    }

    public async Task UpdateAsync(Bom bom, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync("""
            UPDATE Boms SET ProductCode=@ProductCode,ProductName=@ProductName,
            Version=@Version,IsActive=@IsActive,Remark=@Remark WHERE Id=@Id
            """, bom, tx);
        await conn.ExecuteAsync("DELETE FROM BomItems WHERE BomId = @Id", new { bom.Id }, tx);
        foreach (var item in bom.Items)
        {
            item.BomId = bom.Id;
            await conn.ExecuteAsync("""
                INSERT INTO BomItems (BomId,ItemNo,MaterialCode,MaterialName,Quantity,Unit,Remark)
                VALUES (@BomId,@ItemNo,@MaterialCode,@MaterialName,@Quantity,@Unit,@Remark)
                """, item, tx);
        }
        tx.Commit();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync("DELETE FROM BomItems WHERE BomId = @id", new { id }, tx);
        await conn.ExecuteAsync("DELETE FROM Boms WHERE Id = @id", new { id }, tx);
        tx.Commit();
    }

    public async Task<string> NextBomNoAsync(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Boms");
        return $"BOM-{DateTime.Today:yyyy}-{(count + 1):D3}";
    }
}

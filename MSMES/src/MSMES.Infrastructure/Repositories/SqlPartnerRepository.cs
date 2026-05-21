using Dapper;
using MSMES.Domain.Partner;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public class SqlPartnerRepository : IPartnerRepository
{
    private readonly ISqlConnectionFactory _db;
    public SqlPartnerRepository(ISqlConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<Partner>> ListAsync(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync<Partner>("SELECT * FROM Partners ORDER BY PartnerType, PartnerName");
        return rows.ToList();
    }

    public async Task<Partner?> GetAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        return await conn.QueryFirstOrDefaultAsync<Partner>("SELECT * FROM Partners WHERE Id = @id", new { id });
    }

    public async Task<int> CreateAsync(Partner partner, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO Partners (PartnerCode,PartnerName,PartnerType,BusinessNo,RepName,Tel,Email,
                Address,ContactName,ContactTel,PaymentTerms,CreditLimit,Rating,IsActive,Memo,CreatedAt,CreatedBy)
            VALUES (@PartnerCode,@PartnerName,@PartnerType,@BusinessNo,@RepName,@Tel,@Email,
                @Address,@ContactName,@ContactTel,@PaymentTerms,@CreditLimit,@Rating,@IsActive,@Memo,@CreatedAt,@CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, partner);
    }

    public async Task UpdateAsync(Partner partner, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        await conn.ExecuteAsync("""
            UPDATE Partners SET PartnerName=@PartnerName,PartnerType=@PartnerType,BusinessNo=@BusinessNo,
            RepName=@RepName,Tel=@Tel,Email=@Email,Address=@Address,ContactName=@ContactName,
            ContactTel=@ContactTel,PaymentTerms=@PaymentTerms,CreditLimit=@CreditLimit,
            Rating=@Rating,IsActive=@IsActive,Memo=@Memo WHERE Id=@Id
            """, partner);
    }

    public async Task<string> NextPartnerCodeAsync(string type, CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var prefix = type == "CUSTOMER" ? "C" : type == "SUPPLIER" ? "S" : "B";
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Partners WHERE PartnerType = @type", new { type });
        return $"{prefix}-{(count + 1):D3}";
    }
}

using Dapper;
using MSMES.Domain.LotManagement;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlLotRepository : ILotRepository
{
    private readonly ISqlConnectionFactory _factory;
    public SqlLotRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<Lot?> GetByNoAsync(string lotNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Lot>(new CommandDefinition(
            "SELECT * FROM dbo.Lots WHERE LotNo=@no", new { no = lotNo }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Lot>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<Lot>(new CommandDefinition(
            "SELECT * FROM dbo.Lots ORDER BY ProductionDate DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY",
            new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<LotHistory>> GetHistoryAsync(string lotNo, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<LotHistory>(new CommandDefinition(
            "SELECT * FROM dbo.LotHistories WHERE LotNo=@no ORDER BY OperatedAt",
            new { no = lotNo }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(Lot lot, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.Lots
            (LotNo, ItemCode, WorkOrderNo, ProducedQuantity, ProductionDate, Status, CreatedAt, CreatedBy)
            VALUES (@LotNo,@ItemCode,@WorkOrderNo,@ProducedQuantity,@ProductionDate,@Status,@CreatedAt,@CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, lot, cancellationToken: ct));
    }

    public async Task UpdateAsync(Lot lot, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.Lots SET ItemCode=@ItemCode, WorkOrderNo=@WorkOrderNo,
            ProducedQuantity=@ProducedQuantity, ProductionDate=@ProductionDate, Status=@Status,
            UpdatedAt=SYSUTCDATETIME() WHERE LotNo=@LotNo";
        await conn.ExecuteAsync(new CommandDefinition(sql, lot, cancellationToken: ct));
    }

    public async Task AddHistoryAsync(LotHistory h, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.LotHistories
            (LotNo, Operation, OperatedAt, Operator, Remarks, CreatedAt, CreatedBy)
            VALUES (@LotNo, @Operation, @OperatedAt, @Operator, @Remarks, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, h, cancellationToken: ct));
    }

    public Task<string> NextNumberAsync(CancellationToken ct = default) =>
        NumberSequence.NextAsync(_factory, "LOT", "LOT", ct);
}

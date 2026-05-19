using MSMES.Domain.LotManagement;

namespace MSMES.Application.LotManagement;

public sealed record CreateLotCommand(string ItemCode, string WorkOrderNo, decimal ProducedQuantity, DateTime ProductionDate, string CreatedBy);

public sealed class CreateLotHandler
{
    private readonly ILotRepository _repo;
    public CreateLotHandler(ILotRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreateLotCommand cmd, CancellationToken ct = default)
    {
        var no = await _repo.NextNumberAsync(ct);
        var lot = new Lot
        {
            LotNo = no,
            ItemCode = cmd.ItemCode,
            WorkOrderNo = cmd.WorkOrderNo,
            ProducedQuantity = cmd.ProducedQuantity,
            ProductionDate = cmd.ProductionDate,
            Status = LotStatus.Created,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(lot, ct);
        await _repo.AddHistoryAsync(new LotHistory
        {
            LotNo = no, Operation = "Created", OperatedAt = DateTime.UtcNow,
            Operator = cmd.CreatedBy, CreatedAt = DateTime.UtcNow, CreatedBy = cmd.CreatedBy
        }, ct);
        return no;
    }
}

public sealed record GetLotHistoryQuery(string LotNo);

public sealed class GetLotHistoryHandler
{
    private readonly ILotRepository _repo;
    public GetLotHistoryHandler(ILotRepository repo) => _repo = repo;

    public Task<IReadOnlyList<LotHistory>> HandleAsync(GetLotHistoryQuery q, CancellationToken ct = default)
        => _repo.GetHistoryAsync(q.LotNo, ct);
}

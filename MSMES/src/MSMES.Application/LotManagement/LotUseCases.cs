using MSMES.Domain.LotManagement;

namespace MSMES.Application.LotManagement;

public sealed record CreateLotCommand(string ItemCode, string ItemName, string WorkOrderNo, decimal ProducedQuantity, decimal DefectQuantity, DateTime ProductionDate, string CreatedBy);

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
            ItemName = cmd.ItemName,
            WorkOrderNo = cmd.WorkOrderNo,
            ProducedQuantity = cmd.ProducedQuantity,
            DefectQuantity = cmd.DefectQuantity,
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

public sealed record UpdateLotStatusCommand(string LotNo, LotStatus NewStatus, string Operator);

public sealed class UpdateLotStatusHandler
{
    private readonly ILotRepository _repo;
    public UpdateLotStatusHandler(ILotRepository repo) => _repo = repo;

    public async Task HandleAsync(UpdateLotStatusCommand cmd, CancellationToken ct = default)
    {
        var lot = await _repo.GetByNoAsync(cmd.LotNo, ct)
            ?? throw new KeyNotFoundException($"LOT {cmd.LotNo} 을(를) 찾을 수 없습니다.");
        lot.Status = cmd.NewStatus;
        lot.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(lot, ct);
        await _repo.AddHistoryAsync(new LotHistory
        {
            LotNo = cmd.LotNo,
            Operation = $"상태변경: {cmd.NewStatus}",
            OperatedAt = DateTime.UtcNow,
            Operator = cmd.Operator,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = cmd.Operator
        }, ct);
    }
}

public sealed class ListAllLotsHandler
{
    private readonly ILotRepository _repo;
    public ListAllLotsHandler(ILotRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Lot>> HandleAsync(CancellationToken ct = default)
        => _repo.ListAllAsync(ct);
}

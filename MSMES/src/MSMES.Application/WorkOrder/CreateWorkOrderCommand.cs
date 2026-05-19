using MSMES.Domain.WorkOrder;

namespace MSMES.Application.WorkOrder;

public sealed record CreateWorkOrderCommand(
    string? SalesOrderNo,
    string ItemCode,
    decimal PlannedQuantity,
    DateTime StartDate,
    DateTime PlannedEndDate,
    string CreatedBy);

public sealed class CreateWorkOrderHandler
{
    private readonly IWorkOrderRepository _repo;
    public CreateWorkOrderHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreateWorkOrderCommand cmd, CancellationToken ct = default)
    {
        var no = await _repo.NextNumberAsync(ct);
        var w = new Domain.WorkOrder.WorkOrder
        {
            WorkOrderNo = no,
            SalesOrderNo = cmd.SalesOrderNo,
            ItemCode = cmd.ItemCode,
            PlannedQuantity = cmd.PlannedQuantity,
            StartDate = cmd.StartDate,
            PlannedEndDate = cmd.PlannedEndDate,
            Status = WorkOrderStatus.Planned,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(w, ct);
        return no;
    }
}

public sealed record UpdateWorkOrderStatusCommand(string WorkOrderNo, WorkOrderStatus NewStatus, decimal? ProducedQuantity = null);

public sealed class UpdateWorkOrderStatusHandler
{
    private readonly IWorkOrderRepository _repo;
    public UpdateWorkOrderStatusHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task HandleAsync(UpdateWorkOrderStatusCommand cmd, CancellationToken ct = default)
    {
        var w = await _repo.GetByNoAsync(cmd.WorkOrderNo, ct)
            ?? throw new InvalidOperationException("WorkOrder not found");
        switch (cmd.NewStatus)
        {
            case WorkOrderStatus.Released: w.Release(); break;
            case WorkOrderStatus.InProgress: w.Start(); break;
            case WorkOrderStatus.Completed: w.Complete(cmd.ProducedQuantity ?? w.PlannedQuantity); break;
            case WorkOrderStatus.Closed: w.Close(); break;
            default: throw new InvalidOperationException($"Unsupported transition to {cmd.NewStatus}");
        }
        await _repo.UpdateAsync(w, ct);
    }
}

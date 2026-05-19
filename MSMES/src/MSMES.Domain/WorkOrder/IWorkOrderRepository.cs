namespace MSMES.Domain.WorkOrder;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByNoAsync(string workOrderNo, CancellationToken ct = default);
    Task<IReadOnlyList<WorkOrder>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<WorkOrder>> ListBySalesOrderAsync(string salesOrderNo, CancellationToken ct = default);
    Task AddAsync(WorkOrder workOrder, CancellationToken ct = default);
    Task UpdateAsync(WorkOrder workOrder, CancellationToken ct = default);
    Task<string> NextNumberAsync(CancellationToken ct = default);
}

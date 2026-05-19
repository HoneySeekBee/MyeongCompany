namespace MSMES.Domain.SalesOrder;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByNoAsync(string salesOrderNo, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrder>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task AddAsync(SalesOrder order, CancellationToken ct = default);
    Task UpdateAsync(SalesOrder order, CancellationToken ct = default);
    Task<string> NextNumberAsync(CancellationToken ct = default);
}

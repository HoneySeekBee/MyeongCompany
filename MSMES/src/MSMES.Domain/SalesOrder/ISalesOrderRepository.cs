namespace MSMES.Domain.SalesOrder;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByNoAsync(string salesOrderNo, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrder>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrder>> ListFilteredAsync(string? orderNo, string? customer, string? status, DateTime? from, DateTime? to, int skip, int take, CancellationToken ct = default);
    Task AddAsync(SalesOrder order, CancellationToken ct = default);
    Task UpdateAsync(SalesOrder order, CancellationToken ct = default);
    Task DeleteAsync(string salesOrderNo, CancellationToken ct = default);
    Task<string> NextNumberAsync(CancellationToken ct = default);
}

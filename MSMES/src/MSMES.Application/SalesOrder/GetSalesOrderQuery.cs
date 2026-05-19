using MSMES.Domain.SalesOrder;

namespace MSMES.Application.SalesOrder;

public sealed record GetSalesOrderQuery(string SalesOrderNo);

public sealed class GetSalesOrderHandler
{
    private readonly ISalesOrderRepository _repo;
    public GetSalesOrderHandler(ISalesOrderRepository repo) => _repo = repo;

    public Task<Domain.SalesOrder.SalesOrder?> HandleAsync(GetSalesOrderQuery q, CancellationToken ct = default)
        => _repo.GetByNoAsync(q.SalesOrderNo, ct);
}

public sealed record ListSalesOrdersQuery(int Skip = 0, int Take = 50);

public sealed class ListSalesOrdersHandler
{
    private readonly ISalesOrderRepository _repo;
    public ListSalesOrdersHandler(ISalesOrderRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Domain.SalesOrder.SalesOrder>> HandleAsync(ListSalesOrdersQuery q, CancellationToken ct = default)
        => _repo.ListAsync(q.Skip, q.Take, ct);
}

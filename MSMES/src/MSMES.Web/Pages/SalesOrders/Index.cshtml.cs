using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.SalesOrder;

namespace MSMES.Web.Pages.SalesOrders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ISalesOrderRepository _repo;
    public IndexModel(ISalesOrderRepository repo) => _repo = repo;

    public IReadOnlyList<SalesOrder> Orders { get; private set; } = Array.Empty<SalesOrder>();

    public async Task OnGetAsync(
        string? orderNo,
        string? customer,
        string? status,
        string? dateFrom,
        string? dateTo,
        CancellationToken ct)
    {
        DateTime? from = dateFrom is not null && DateTime.TryParse(dateFrom, out var fd) ? fd : null;
        DateTime? to   = dateTo   is not null && DateTime.TryParse(dateTo,   out var td) ? td : null;

        Orders = await _repo.ListFilteredAsync(orderNo, customer, status, from, to, 0, 100, ct);
    }
}

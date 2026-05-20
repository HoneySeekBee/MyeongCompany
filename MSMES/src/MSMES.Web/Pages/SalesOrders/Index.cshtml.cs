using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Application.SalesOrder;

namespace MSMES.Web.Pages.SalesOrders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ListSalesOrdersHandler _list;
    public IndexModel(ListSalesOrdersHandler list) => _list = list;

    public IReadOnlyList<MSMES.Domain.SalesOrder.SalesOrder> Orders { get; private set; } = Array.Empty<MSMES.Domain.SalesOrder.SalesOrder>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Orders = await _list.HandleAsync(new ListSalesOrdersQuery(0, 50), ct);
    }
}

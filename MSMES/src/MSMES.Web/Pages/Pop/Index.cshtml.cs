using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.WorkOrder;

namespace MSMES.Web.Pages.Pop;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IWorkOrderRepository _repo;

    public IndexModel(IWorkOrderRepository repo) => _repo = repo;

    public IReadOnlyList<WorkOrder> ActiveOrders { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        var all = await _repo.ListAsync(0, 200, ct);
        ActiveOrders = all
            .Where(w => w.Status == WorkOrderStatus.Released || w.Status == WorkOrderStatus.InProgress)
            .OrderBy(w => w.Status)          // Released 먼저, InProgress 다음
            .ThenBy(w => w.StartDate)
            .ToList();
    }
}

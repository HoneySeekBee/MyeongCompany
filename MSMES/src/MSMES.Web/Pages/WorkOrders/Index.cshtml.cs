using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.WorkOrder;

namespace MSMES.Web.Pages.WorkOrders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IWorkOrderRepository _repo;

    public IndexModel(IWorkOrderRepository repo) => _repo = repo;

    public int TotalCount { get; private set; }
    public int PlannedCount { get; private set; }
    public int InProgressCount { get; private set; }
    public int CompletedCount { get; private set; }
    public IReadOnlyList<WorkOrder> WorkOrders { get; private set; } = [];

    public async Task OnGetAsync(
        string? woNo, string? item, string? status,
        CancellationToken ct)
    {
        WorkOrders = await _repo.ListFilteredAsync(woNo, item, status, 0, 100, ct);
        TotalCount      = WorkOrders.Count;
        PlannedCount    = WorkOrders.Count(w => w.Status == WorkOrderStatus.Planned);
        InProgressCount = WorkOrders.Count(w =>
            w.Status == WorkOrderStatus.InProgress || w.Status == WorkOrderStatus.Released);
        CompletedCount  = WorkOrders.Count(w => w.Status == WorkOrderStatus.Completed);
    }
}

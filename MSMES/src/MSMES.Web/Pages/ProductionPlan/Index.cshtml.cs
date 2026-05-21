using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.ProductionPlan;

namespace MSMES.Web.Pages.ProductionPlan;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProductionPlanRepository _repo;
    public IndexModel(IProductionPlanRepository repo) => _repo = repo;

    public IReadOnlyList<MSMES.Domain.ProductionPlan.ProductionPlan> Plans { get; private set; } = [];
    public DateTime GanttStart { get; private set; }
    public DateTime GanttEnd   { get; private set; }
    public int TotalDays       { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        GanttStart = DateTime.Today.AddDays(-7);
        GanttEnd   = DateTime.Today.AddDays(21);
        TotalDays  = (GanttEnd - GanttStart).Days + 1;
        Plans = await _repo.ListAsync(GanttStart, GanttEnd, ct);
    }
}

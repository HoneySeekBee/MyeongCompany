using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Application.LotManagement;
using MSMES.Domain.LotManagement;

namespace MSMES.Web.Pages.Lots;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ListAllLotsHandler _listAll;

    public IndexModel(ListAllLotsHandler listAll) => _listAll = listAll;

    public IReadOnlyList<Lot> Lots { get; private set; } = Array.Empty<Lot>();

    public int TotalCount    { get; private set; }
    public int CreatedCount  { get; private set; }
    public int InProcessCount { get; private set; }
    public int ReleasedCount { get; private set; }
    public int ShippedCount  { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Lots = await _listAll.HandleAsync(ct);
        TotalCount    = Lots.Count;
        CreatedCount  = Lots.Count(l => l.Status == LotStatus.Created);
        InProcessCount = Lots.Count(l => l.Status == LotStatus.InProcess);
        ReleasedCount = Lots.Count(l => l.Status == LotStatus.Released);
        ShippedCount  = Lots.Count(l => l.Status == LotStatus.Shipped);
    }
}

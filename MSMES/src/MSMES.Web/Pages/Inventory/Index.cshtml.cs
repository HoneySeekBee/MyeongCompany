using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Application.Inventory;
using MSMES.Domain.Inventory;

namespace MSMES.Web.Pages.Inventory;

public class IndexModel : PageModel
{
    private readonly GetInventoryStatusHandler _status;
    private readonly IInventoryRepository _repo;

    public IndexModel(GetInventoryStatusHandler status, IInventoryRepository repo)
    {
        _status = status;
        _repo = repo;
    }

    public IReadOnlyList<Domain.Inventory.Inventory> Inventories { get; private set; }
        = Array.Empty<Domain.Inventory.Inventory>();

    public int TotalCount { get; private set; }
    public int LowStockCount { get; private set; }
    public int OutOfStockCount { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Inventories = await _status.HandleAsync(new GetInventoryStatusQuery(null, 0, 200), ct);

        var summary = await _repo.GetStatusSummaryAsync(ct);
        TotalCount = summary.Normal + summary.Low + summary.Out;
        LowStockCount = summary.Low;
        OutOfStockCount = summary.Out;
    }
}

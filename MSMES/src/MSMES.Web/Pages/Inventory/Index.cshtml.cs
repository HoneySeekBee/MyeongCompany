using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Application.Inventory;
using MSMES.Domain.Inventory;

namespace MSMES.Web.Pages.Inventory;

[Authorize]
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

    public IReadOnlyList<InventoryTransaction> Transactions { get; private set; }
        = Array.Empty<InventoryTransaction>();

    public IReadOnlyList<Domain.Inventory.Inventory> LowStockItems { get; private set; }
        = Array.Empty<Domain.Inventory.Inventory>();

    public int TotalCount { get; private set; }
    public int NormalCount { get; private set; }
    public int LowStockCount { get; private set; }
    public int OutOfStockCount { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Inventories = await _status.HandleAsync(new GetInventoryStatusQuery(null, 0, 200), ct);
        Transactions = await _repo.GetRecentTransactionsAsync(50, ct);

        LowStockItems = Inventories
            .Where(i => i.Status == InventoryStatus.LowStock || i.Status == InventoryStatus.OutOfStock)
            .OrderBy(i => i.Status)   // OutOfStock(2) last for visual priority — reversed below
            .ThenBy(i => i.ItemCode)
            .ToList();

        var summary = await _repo.GetStatusSummaryAsync(ct);
        NormalCount = summary.Normal;
        LowStockCount = summary.Low;
        OutOfStockCount = summary.Out;
        TotalCount = summary.Normal + summary.Low + summary.Out;
    }
}

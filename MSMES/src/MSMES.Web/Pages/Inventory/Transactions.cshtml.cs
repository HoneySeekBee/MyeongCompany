using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Inventory;

namespace MSMES.Web.Pages.Inventory;

[Authorize]
public class TransactionsModel : PageModel
{
    private readonly IInventoryRepository _repo;

    public TransactionsModel(IInventoryRepository repo) => _repo = repo;

    public IReadOnlyList<InventoryTransaction> Transactions { get; private set; } = [];
    public IReadOnlyList<Domain.Inventory.Inventory> Inventories { get; private set; } = [];
    public string? FilterItemCode { get; private set; }

    // KPI 카드
    public int TodayIn  { get; private set; }
    public int TodayOut { get; private set; }
    public int TodayAdj { get; private set; }

    public async Task OnGetAsync(string? itemCode, CancellationToken ct)
    {
        FilterItemCode = itemCode;

        Transactions = await _repo.ListTransactionsAsync(itemCode, 0, 200, ct);
        Inventories  = await _repo.ListAsync(0, 500, ct);

        var today = await _repo.GetTodayTransactionCountsAsync(ct);
        TodayIn  = today.TodayIn;
        TodayOut = today.TodayOut;
        TodayAdj = today.TodayAdj;
    }
}

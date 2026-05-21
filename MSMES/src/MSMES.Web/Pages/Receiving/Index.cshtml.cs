using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Receiving;

namespace MSMES.Web.Pages.Receiving;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IGoodsReceiptRepository _repo;
    public IndexModel(IGoodsReceiptRepository repo) => _repo = repo;

    public IReadOnlyList<GoodsReceipt> Receipts    { get; private set; } = [];
    public int PendingCount     { get; private set; }
    public int InspectingCount  { get; private set; }
    public int PassedCount      { get; private set; }
    public int FailedCount      { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Receipts       = await _repo.ListAsync(ct);
        PendingCount   = Receipts.Count(r => r.Status == 0);
        InspectingCount= Receipts.Count(r => r.Status == 1);
        PassedCount    = Receipts.Count(r => r.Status == 2 || r.Status == 4);
        FailedCount    = Receipts.Count(r => r.Status == 3);
    }
}

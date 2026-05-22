using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.PurchaseOrder;

namespace MSMES.Web.Pages.PurchaseOrders;

[Authorize]
public class PrintModel : PageModel
{
    private readonly IPurchaseOrderRepository _repo;

    public PrintModel(IPurchaseOrderRepository repo) => _repo = repo;

    public PurchaseOrder? PurchaseOrder { get; private set; }

    public async Task<IActionResult> OnGetAsync(string no, CancellationToken ct)
    {
        PurchaseOrder = await _repo.GetWithItemsAsync(no, ct);
        if (PurchaseOrder is null) return NotFound();
        ViewData["Title"] = $"발주서 - {no}";
        return Page();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.WorkOrder;

namespace MSMES.Web.Pages.WorkOrders;

[Authorize]
public class PrintModel : PageModel
{
    private readonly IWorkOrderRepository _repo;

    public PrintModel(IWorkOrderRepository repo) => _repo = repo;

    public WorkOrder? WorkOrder { get; private set; }

    public async Task<IActionResult> OnGetAsync(string no, CancellationToken ct)
    {
        WorkOrder = await _repo.GetByNoAsync(no, ct);
        if (WorkOrder is null) return NotFound();
        ViewData["Title"] = $"작업지시서 - {no}";
        return Page();
    }
}

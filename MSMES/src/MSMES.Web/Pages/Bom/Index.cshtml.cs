using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Bom;

namespace MSMES.Web.Pages.Bom;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IBomRepository _repo;
    public IndexModel(IBomRepository repo) => _repo = repo;

    public IReadOnlyList<MSMES.Domain.Bom.Bom> Boms { get; private set; } = [];
    public int ActiveCount   { get; private set; }
    public int TotalItems    { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Boms         = await _repo.ListAsync(ct);
        ActiveCount  = Boms.Count(b => b.IsActive);
        TotalItems   = Boms.Sum(b => b.Items.Count);
    }
}

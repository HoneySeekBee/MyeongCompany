using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Partner;

namespace MSMES.Web.Pages.Partners;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IPartnerRepository _repo;
    public IndexModel(IPartnerRepository repo) => _repo = repo;

    public IReadOnlyList<Partner> Partners      { get; private set; } = [];
    public int CustomerCount  { get; private set; }
    public int SupplierCount  { get; private set; }
    public int BothCount      { get; private set; }
    public int ActiveCount    { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Partners      = await _repo.ListAsync(ct);
        CustomerCount = Partners.Count(p => p.PartnerType == "CUSTOMER");
        SupplierCount = Partners.Count(p => p.PartnerType == "SUPPLIER");
        BothCount     = Partners.Count(p => p.PartnerType == "BOTH");
        ActiveCount   = Partners.Count(p => p.IsActive);
    }
}

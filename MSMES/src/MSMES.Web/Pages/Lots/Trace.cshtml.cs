using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.LotManagement;

namespace MSMES.Web.Pages.Lots;

[Authorize]
public class TraceModel : PageModel
{
    private readonly ILotRepository _repo;
    public TraceModel(ILotRepository repo) => _repo = repo;

    [BindProperty(SupportsGet = true)] public string? SearchLotNo       { get; set; }
    [BindProperty(SupportsGet = true)] public string? SearchMaterialCode { get; set; }

    public Lot?                       FoundLot     { get; private set; }
    public IReadOnlyList<LotHistory>  LotHistories { get; private set; } = [];
    public IReadOnlyList<LotMaterial> Materials    { get; private set; } = [];
    public IReadOnlyList<string>      RelatedLots  { get; private set; } = [];  // 역추적: 같은 자재 사용 LOT

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(SearchLotNo))
        {
            var allLots = await _repo.ListAllAsync(ct);
            FoundLot = allLots.FirstOrDefault(l => l.LotNo.Contains(SearchLotNo.Trim(), StringComparison.OrdinalIgnoreCase));
            if (FoundLot is not null)
            {
                LotHistories = await _repo.GetHistoryAsync(FoundLot.LotNo, ct);
                Materials    = await _repo.GetLotMaterialsAsync(FoundLot.LotNo, ct);
            }
        }
        else if (!string.IsNullOrWhiteSpace(SearchMaterialCode))
        {
            RelatedLots = await _repo.FindLotsByMaterialAsync(SearchMaterialCode.Trim(), ct);
        }
    }
}

using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Quality;

namespace MSMES.Web.Pages.Quality;

public class IndexModel : PageModel
{
    private readonly IQualityRepository _repo;

    public IndexModel(IQualityRepository repo) => _repo = repo;

    public IReadOnlyList<QualityInspection> Inspections { get; private set; }
        = Array.Empty<QualityInspection>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Inspections = await _repo.ListAsync(0, 100, ct);
    }
}

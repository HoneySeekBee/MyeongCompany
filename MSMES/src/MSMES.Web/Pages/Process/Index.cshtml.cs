using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Process;

namespace MSMES.Web.Pages.Process;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProcessRepository _repo;

    public IReadOnlyList<ProcessDefinition> Definitions { get; private set; } = [];

    /// <summary>공정별 금일 생산실적 집계 (ProcessCode → (Produced, Defect))</summary>
    public IReadOnlyList<ProcessDailySummary> DailySummary { get; private set; } = [];

    public IndexModel(IProcessRepository repo) => _repo = repo;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Definitions = await _repo.ListProcessesAsync(ct);

        var today = DateTime.UtcNow.Date;
        var results = await _repo.ListResultsAsync(today, today.AddDays(1), ct);

        DailySummary = results
            .GroupBy(r => r.ProcessCode)
            .Select(g => new ProcessDailySummary(
                g.Key,
                Definitions.FirstOrDefault(d => d.ProcessCode == g.Key)?.ProcessName ?? g.Key,
                g.Sum(r => r.ProducedQuantity),
                g.Sum(r => r.DefectQuantity)))
            .OrderBy(s => s.ProcessCode)
            .ToList();
    }
}

public sealed record ProcessDailySummary(
    string ProcessCode,
    string ProcessName,
    decimal Produced,
    decimal Defect)
{
    public decimal DefectRate => Produced == 0 ? 0 : Math.Round(Defect / Produced * 100m, 1);
    public decimal AchievementRate => Produced == 0 ? 0 : Math.Min(100m, Math.Round(Produced / (Produced + Defect) * 100m, 1));
}

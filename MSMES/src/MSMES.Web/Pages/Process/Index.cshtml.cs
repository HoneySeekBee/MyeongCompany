using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Process;

namespace MSMES.Web.Pages.Process;

public class IndexModel : PageModel
{
    private readonly IProcessRepository _repo;

    public IReadOnlyList<ProcessDefinition> ProcessDefinitions { get; private set; } = [];
    public IReadOnlyList<ProductionResult>  ProductionResults  { get; private set; } = [];

    // 생산 실적 탭 요약
    public decimal TodayProduced  { get; private set; }
    public decimal TodayDefect    { get; private set; }
    public decimal TodayDefectRate => TodayProduced == 0
        ? 0m
        : Math.Round(TodayDefect / TodayProduced * 100m, 1);

    // 공정 정의 탭 요약
    public int     TotalProcessCount        => ProcessDefinitions.Count;
    public decimal AverageStandardTimeMinutes =>
        ProcessDefinitions.Count == 0
            ? 0m
            : Math.Round(ProcessDefinitions.Average(d => d.StandardTimeMinutes), 1);

    public IndexModel(IProcessRepository repo) => _repo = repo;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ProcessDefinitions = await _repo.ListProcessesAsync(ct);

        var today = DateTime.UtcNow.Date;
        ProductionResults = await _repo.ListResultsAsync(today, today.AddDays(1), ct);

        TodayProduced = ProductionResults.Sum(r => r.ProducedQuantity);
        TodayDefect   = ProductionResults.Sum(r => r.DefectQuantity);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Quality;

namespace MSMES.Web.Pages.Quality;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IQualityRepository _repo;

    public IndexModel(IQualityRepository repo) => _repo = repo;

    // ── 검사 목록 ──────────────────────────────────────────
    public IReadOnlyList<QualityInspection> Inspections { get; private set; }
        = Array.Empty<QualityInspection>();

    // ── KPI 카드 ───────────────────────────────────────────
    public int TotalCount         { get; private set; }
    public int PassedCount        { get; private set; }
    public int FailedCount        { get; private set; }
    public int PendingCount       { get; private set; }
    public int ConditionalPassCount { get; private set; }
    public decimal PassRate       { get; private set; }   // 합격률 %

    // ── 불량률 추이 (최근 7일) ─────────────────────────────
    public List<(string Date, decimal DefectRate)> DefectTrend { get; private set; } = new();

    // ── 불량 유형 목록 (등록 모달 select) ─────────────────
    public IReadOnlyList<DefectType> DefectTypes { get; private set; }
        = Array.Empty<DefectType>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        // 1. 전체 검사 목록 (최대 5000건)
        Inspections = await _repo.ListAsync(0, 5000, ct);

        // 2. KPI 집계
        TotalCount          = Inspections.Count;
        PassedCount         = Inspections.Count(i => i.Result == QualityStatus.Passed);
        FailedCount         = Inspections.Count(i => i.Result == QualityStatus.Failed);
        PendingCount        = Inspections.Count(i => i.Result == QualityStatus.Pending);
        ConditionalPassCount = Inspections.Count(i => i.Result == QualityStatus.ConditionalPass);
        PassRate            = TotalCount == 0 ? 0m
                              : Math.Round((decimal)PassedCount / TotalCount * 100m, 1);

        // 3. 최근 7일 불량률 추이
        var today = DateTime.UtcNow.Date;
        for (int d = 6; d >= 0; d--)
        {
            var day = today.AddDays(-d);
            var dayLabel = day.ToString("MM/dd");
            var dayItems = Inspections
                .Where(i => i.InspectionDate.Date == day)
                .ToList();

            decimal rate = 0m;
            if (dayItems.Count > 0)
            {
                var totalInspected = dayItems.Sum(i => i.InspectedQuantity);
                var totalDefect    = dayItems.Sum(i => i.DefectQuantity);
                rate = totalInspected == 0 ? 0m
                       : Math.Round(totalDefect / totalInspected * 100m, 2);
            }
            DefectTrend.Add((dayLabel, rate));
        }

        // 4. 불량 유형
        DefectTypes = await _repo.ListDefectTypesAsync(ct);
    }
}

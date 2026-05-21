using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Spc;

namespace MSMES.Web.Pages.Quality;

[Authorize]
public class SpcModel : PageModel
{
    private readonly ISpcRepository _repo;
    public SpcModel(ISpcRepository repo) => _repo = repo;

    public IReadOnlyList<SpcMeasurement> Measurements { get; private set; } = [];
    public IReadOnlyList<string>         ProcessList  { get; private set; } = [];
    public string SelectedProcess { get; private set; } = "P-DRILL-01";

    // SPC statistics
    public decimal Xbar     { get; private set; }  // 전체 평균
    public decimal Rbar     { get; private set; }  // 평균 범위
    public decimal UCL_X    { get; private set; }  // X-bar 관리상한
    public decimal LCL_X    { get; private set; }  // X-bar 관리하한
    public decimal UCL_R    { get; private set; }  // R 관리상한
    public decimal LCL_R    { get; private set; }  // R 관리하한
    public decimal USL      { get; private set; }
    public decimal LSL      { get; private set; }
    public decimal Cp       { get; private set; }  // 공정능력지수
    public decimal Cpk      { get; private set; }  // 수정 공정능력지수
    public int     OutOfControlCount { get; private set; }

    // Chart data (JSON serializable)
    public List<decimal> SubgroupMeans  { get; private set; } = [];
    public List<decimal> SubgroupRanges { get; private set; } = [];
    public List<int>     SubgroupNos    { get; private set; } = [];

    // A2, D3, D4 constants for n=5
    private const decimal A2 = 0.577m;
    private const decimal D3 = 0.000m;
    private const decimal D4 = 2.114m;

    public async Task OnGetAsync([Microsoft.AspNetCore.Mvc.FromQuery] string? process, CancellationToken ct = default)
    {
        ProcessList = await _repo.GetProcessListAsync(ct);
        if (!string.IsNullOrEmpty(process)) SelectedProcess = process;

        Measurements = await _repo.GetByProcessAsync(SelectedProcess, 25, ct);
        if (!Measurements.Any()) return;

        USL = Measurements.First().UpperSpec;
        LSL = Measurements.First().LowerSpec;

        // Group by subgroup
        var groups = Measurements
            .GroupBy(m => m.SubgroupNo)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var g in groups)
        {
            var vals = g.Select(m => m.MeasuredValue).ToList();
            var mean = vals.Average();
            var range = vals.Max() - vals.Min();
            SubgroupMeans.Add(Math.Round(mean, 4));
            SubgroupRanges.Add(Math.Round(range, 4));
            SubgroupNos.Add(g.Key);
        }

        Xbar = Math.Round(SubgroupMeans.Average(), 4);
        Rbar = Math.Round(SubgroupRanges.Average(), 4);
        UCL_X = Math.Round(Xbar + A2 * Rbar, 4);
        LCL_X = Math.Round(Xbar - A2 * Rbar, 4);
        UCL_R = Math.Round(D4 * Rbar, 4);
        LCL_R = Math.Round(D3 * Rbar, 4);

        // Cp, Cpk
        var sigma = Rbar / 2.326m; // d2 for n=5
        if (sigma > 0)
        {
            Cp  = Math.Round((USL - LSL) / (6 * sigma), 2);
            var cpu = (USL - Xbar) / (3 * sigma);
            var cpl = (Xbar - LSL) / (3 * sigma);
            Cpk = Math.Round(Math.Min(cpu, cpl), 2);
        }

        // Out of control: points beyond control limits
        OutOfControlCount = SubgroupMeans.Count(m => m > UCL_X || m < LCL_X)
                          + SubgroupRanges.Count(r => r > UCL_R);
    }
}

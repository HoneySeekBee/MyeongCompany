using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Equipment;

namespace MSMES.Web.Pages.Equipment;

[Authorize]
public class OeeModel : PageModel
{
    private readonly IEquipmentRepository _repo;
    public OeeModel(IEquipmentRepository repo) => _repo = repo;

    public IReadOnlyList<OeeRecord> Records { get; private set; } = [];
    public decimal AvgAvailability { get; private set; }
    public decimal AvgPerformance  { get; private set; }
    public decimal AvgQualityRate  { get; private set; }
    public decimal AvgOEE          { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var from = DateTime.Today.AddDays(-6);
        var to   = DateTime.Today.AddDays(1);
        Records = await _repo.GetOeeRecordsAsync(from, to, ct);

        if (Records.Any())
        {
            AvgAvailability = Math.Round(Records.Average(r => r.Availability), 1);
            AvgPerformance  = Math.Round(Records.Average(r => r.Performance),  1);
            AvgQualityRate  = Math.Round(Records.Average(r => r.QualityRate),  1);
            AvgOEE          = Math.Round(Records.Average(r => r.OEE),          1);
        }
    }
}

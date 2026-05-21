using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Equipment;

namespace MSMES.Web.Pages.Equipment;

[Authorize]
public class PmModel : PageModel
{
    private readonly IEquipmentRepository _repo;
    public PmModel(IEquipmentRepository repo) => _repo = repo;

    public IReadOnlyList<PmSchedule> Schedules  { get; private set; } = [];
    public IReadOnlyList<PmRecord>   Records    { get; private set; } = [];
    public int OverdueCount  { get; private set; }
    public int DueSoonCount  { get; private set; }
    public int HealthyCount  { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Schedules    = await _repo.GetPmSchedulesAsync(ct);
        Records      = await _repo.GetPmRecordsAsync(null, ct);
        OverdueCount = Schedules.Count(s => s.IsOverdue);
        DueSoonCount = Schedules.Count(s => s.IsDueSoon && !s.IsOverdue);
        HealthyCount = Schedules.Count(s => !s.IsOverdue && !s.IsDueSoon);
    }
}

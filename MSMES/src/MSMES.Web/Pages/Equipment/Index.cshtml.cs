using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Equipment;

namespace MSMES.Web.Pages.Equipment;

public class IndexModel : PageModel
{
    private readonly IEquipmentRepository _repo;

    public IReadOnlyList<Domain.Equipment.Equipment> Equipments      { get; private set; } = [];
    public IReadOnlyList<Domain.Equipment.Equipment> MaintenanceDue  { get; private set; } = [];

    public int RunningCount     { get; private set; }
    public int MaintenanceCount { get; private set; }
    public int BreakdownCount   { get; private set; }
    public int StoppedCount     { get; private set; }

    /// <summary>이번 주 점검 예정 수 (7일 이내)</summary>
    public int MaintenanceDueCount => MaintenanceDue.Count;

    public IndexModel(IEquipmentRepository repo) => _repo = repo;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Equipments    = await _repo.ListAsync(0, 200, ct);
        MaintenanceDue = await _repo.GetMaintenanceDueAsync(ct);

        var summary = await _repo.GetStatusSummaryAsync(ct);
        RunningCount     = summary.Running;
        MaintenanceCount = summary.Maintenance;
        BreakdownCount   = summary.Breakdown;
        StoppedCount     = summary.Stopped;
    }
}

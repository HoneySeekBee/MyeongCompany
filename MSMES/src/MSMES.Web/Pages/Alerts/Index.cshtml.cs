using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Alert;

namespace MSMES.Web.Pages.Alerts;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IAlertRepository _repo;
    public IndexModel(IAlertRepository repo) => _repo = repo;

    public IReadOnlyList<Alert> Alerts   { get; private set; } = [];
    public int DangerCount  { get; private set; }
    public int WarningCount { get; private set; }
    public int UnreadCount  { get; private set; }
    public bool ShowResolved { get; private set; }

    public async Task OnGetAsync([FromQuery] bool showResolved = false, CancellationToken ct = default)
    {
        ShowResolved = showResolved;
        Alerts       = await _repo.ListAsync(showResolved, ct);
        DangerCount  = Alerts.Count(a => a.Severity == 3 && !a.IsResolved);
        WarningCount = Alerts.Count(a => a.Severity == 2 && !a.IsResolved);
        UnreadCount  = Alerts.Count(a => !a.IsRead && !a.IsResolved);
    }
}

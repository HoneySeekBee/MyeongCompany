using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Common;

namespace MSMES.Web.Pages.Admin.AuditLogs;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    private readonly IAuditLogRepository _repo;
    public IndexModel(IAuditLogRepository repo) => _repo = repo;

    public IReadOnlyList<AuditLog> Logs { get; private set; } = [];
    public int TotalCount { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Logs = await _repo.ListAsync(0, 100, ct);
        TotalCount = Logs.Count;
    }
}

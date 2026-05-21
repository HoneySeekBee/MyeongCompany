using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Domain.Alert;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IAlertRepository _repo;
    public AlertsController(IAlertRepository repo) => _repo = repo;

    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct)
        => Ok(new { count = await _repo.CountUnreadAsync(ct) });

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        await _repo.MarkReadAsync(id, ct);
        return Ok();
    }

    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id, CancellationToken ct)
    {
        await _repo.ResolveAsync(id, ct);
        return Ok();
    }
}

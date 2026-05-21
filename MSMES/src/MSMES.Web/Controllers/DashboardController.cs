using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Dashboard;
using MSMES.Domain.Alert;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardHandler _handler;
    private readonly IAlertRepository _alertRepo;

    public DashboardController(DashboardHandler handler, IAlertRepository alertRepo)
    {
        _handler = handler;
        _alertRepo = alertRepo;
    }

    /// <summary>대시보드 전체 KPI / 차트 / 최근 작업지시 데이터</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] decimal todayTarget = 1000m,
        CancellationToken ct = default)
    {
        var dto = await _handler.HandleAsync(new DashboardQuery(todayTarget), ct);
        return Ok(dto);
    }

    /// <summary>최근 미해결 알람 상위 3건</summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> RecentAlerts(CancellationToken ct = default)
    {
        var alerts = await _alertRepo.ListAsync(false, ct);
        return Ok(alerts.Take(3).Select(a => new {
            a.Id,
            a.Title,
            a.Severity,
            a.AlertType,
            a.SeverityCss,
            a.AlertTypeIcon,
            createdAt = a.CreatedAt
        }));
    }
}

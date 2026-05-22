using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Dashboard;
using MSMES.Domain.Alert;
using MSMES.Domain.SalesOrder;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardHandler _handler;
    private readonly IAlertRepository _alertRepo;
    private readonly ISalesOrderRepository _soRepo;

    public DashboardController(DashboardHandler handler, IAlertRepository alertRepo, ISalesOrderRepository soRepo)
    {
        _handler   = handler;
        _alertRepo = alertRepo;
        _soRepo    = soRepo;
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

    /// <summary>납기 임박/초과 수주 카운트</summary>
    [HttpGet("due-soon")]
    public async Task<IActionResult> DueSoon(CancellationToken ct = default)
    {
        var orders = await _soRepo.ListFilteredAsync(null, null, null, null, null, 0, 200, ct);
        var today  = DateTime.Today;
        return Ok(new
        {
            overdueCount = orders.Count(o => o.DueDate < today &&
                                             o.Status != SalesOrderStatus.Shipped &&
                                             o.Status != SalesOrderStatus.Closed),
            dueSoon3     = orders.Count(o => o.DueDate >= today &&
                                             (o.DueDate - today).Days <= 3 &&
                                             o.Status != SalesOrderStatus.Shipped &&
                                             o.Status != SalesOrderStatus.Closed),
            dueSoon7     = orders.Count(o => o.DueDate >= today &&
                                             (o.DueDate - today).Days <= 7 &&
                                             o.Status != SalesOrderStatus.Shipped &&
                                             o.Status != SalesOrderStatus.Closed)
        });
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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Dashboard;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardHandler _handler;

    public DashboardController(DashboardHandler handler) => _handler = handler;

    /// <summary>대시보드 전체 KPI / 차트 / 최근 작업지시 데이터</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] decimal todayTarget = 1000m,
        CancellationToken ct = default)
    {
        var dto = await _handler.HandleAsync(new DashboardQuery(todayTarget), ct);
        return Ok(dto);
    }
}

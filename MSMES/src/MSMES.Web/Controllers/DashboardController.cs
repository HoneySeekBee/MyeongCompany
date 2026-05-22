using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Dashboard;
using MSMES.Domain.SalesOrder;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardHandler _handler;
    private readonly ISalesOrderRepository _soRepo;
    private readonly ISqlConnectionFactory _db;

    public DashboardController(DashboardHandler handler, ISalesOrderRepository soRepo, ISqlConnectionFactory db)
    {
        _handler = handler;
        _soRepo  = soRepo;
        _db      = db;
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

    /// <summary>
    /// 실시간 상위 3건 알림 (위험 우선) — 라이브 데이터 쿼리 기반
    /// </summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> RecentAlerts(CancellationToken ct = default)
    {
        using var conn = _db.Create();
        var now = DateTime.UtcNow;

        var alerts = new List<object>();

        // 1. 작업지시 지연 (위험)
        var overdueWos = await conn.QueryAsync(new CommandDefinition(@"
            SELECT TOP 3
                WorkOrderNo, ISNULL(ItemName, ItemCode) AS ItemName, PlannedEndDate
            FROM dbo.WorkOrders
            WHERE Status IN (1, 2) AND PlannedEndDate < CAST(GETUTCDATE() AS DATE)
            ORDER BY PlannedEndDate ASC",
            cancellationToken: ct));

        foreach (var wo in overdueWos)
        {
            alerts.Add(new {
                Title       = $"작업지시 지연: {wo.WorkOrderNo}",
                Message     = $"{wo.ItemName} — 계획 완료일 {((DateTime)wo.PlannedEndDate):yyyy-MM-dd} 초과",
                Severity    = 3,
                SeverityCss = "danger",
                AlertType   = "WO_OVERDUE",
                AlertTypeIcon = "bi-exclamation-triangle-fill",
                EntityId    = (string)wo.WorkOrderNo,
                LinkUrl     = "/WorkOrders"
            });
        }

        // 2. 재고 부족 위험 (OutOfStock)
        if (alerts.Count < 3)
        {
            var outStock = await conn.QueryAsync(new CommandDefinition(@"
                SELECT TOP 3 ItemCode, ItemName, CurrentStock FROM dbo.Inventories
                WHERE CurrentStock <= 0 ORDER BY CurrentStock ASC",
                cancellationToken: ct));

            foreach (var inv in outStock.Take(3 - alerts.Count))
            {
                alerts.Add(new {
                    Title       = $"재고 소진: {inv.ItemCode}",
                    Message     = $"{inv.ItemName} — 현재 재고 0 (즉시 발주 필요)",
                    Severity    = 3,
                    SeverityCss = "danger",
                    AlertType   = "STOCK_OUT",
                    AlertTypeIcon = "bi-box-seam",
                    EntityId    = (string)inv.ItemCode,
                    LinkUrl     = "/Inventory"
                });
            }
        }

        // 3. 납기 임박 (경고) — 빈 슬롯 채우기
        if (alerts.Count < 3)
        {
            var dueSoon = await conn.QueryAsync(new CommandDefinition(@"
                SELECT TOP 3
                    WorkOrderNo, ISNULL(ItemName, ItemCode) AS ItemName, PlannedEndDate
                FROM dbo.WorkOrders
                WHERE Status IN (0, 1, 2)
                  AND PlannedEndDate <= CAST(DATEADD(day, 3, GETUTCDATE()) AS DATE)
                  AND PlannedEndDate >= CAST(GETUTCDATE() AS DATE)
                ORDER BY PlannedEndDate ASC",
                cancellationToken: ct));

            foreach (var wo in dueSoon.Take(3 - alerts.Count))
            {
                alerts.Add(new {
                    Title       = $"납기 임박: {wo.WorkOrderNo}",
                    Message     = $"{wo.ItemName} — 완료 예정 {((DateTime)wo.PlannedEndDate):yyyy-MM-dd}",
                    Severity    = 2,
                    SeverityCss = "warning",
                    AlertType   = "DELIVERY_DUE",
                    AlertTypeIcon = "bi-clock-fill",
                    EntityId    = (string)wo.WorkOrderNo,
                    LinkUrl     = "/WorkOrders"
                });
            }
        }

        return Ok(alerts.Take(3));
    }
}

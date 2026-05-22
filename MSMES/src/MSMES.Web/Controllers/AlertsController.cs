using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Domain.Alert;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IAlertRepository _repo;
    private readonly ISqlConnectionFactory _db;

    public AlertsController(IAlertRepository repo, ISqlConnectionFactory db)
    {
        _repo = repo;
        _db   = db;
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /api/alerts/count
    // 실시간 알림 카운트: critical(지연/위험), warning(임박/경고), info(정보), total
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        using var conn = _db.Create();

        // 위험: 작업지시 지연
        var overdueWo = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(*) FROM dbo.WorkOrders
            WHERE Status IN (1, 2) AND PlannedEndDate < CAST(GETUTCDATE() AS DATE)",
            cancellationToken: ct));

        // 위험: 재고 소진
        var stockOut = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(*) FROM dbo.Inventories WHERE CurrentStock <= 0",
            cancellationToken: ct));

        // 경고: 납기 임박 (오늘~+3일)
        var dueSoon = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(*) FROM dbo.WorkOrders
            WHERE Status IN (0, 1, 2)
              AND PlannedEndDate <= CAST(DATEADD(day, 3, GETUTCDATE()) AS DATE)
              AND PlannedEndDate >= CAST(GETUTCDATE() AS DATE)",
            cancellationToken: ct));

        // 경고: 재고 부족 (0 < stock <= 안전재고)
        var stockLow = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(*) FROM dbo.Inventories
            WHERE CurrentStock > 0 AND CurrentStock <= SafetyStock",
            cancellationToken: ct));

        // 정보: 미확정 수주 (3일 초과)
        var unconfirmedSo = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(*) FROM dbo.SalesOrders
            WHERE Status = 0 AND OrderDate <= CAST(DATEADD(day, -3, GETUTCDATE()) AS DATE)",
            cancellationToken: ct));

        var critical = overdueWo + stockOut;
        var warning  = dueSoon + stockLow;
        var info     = unconfirmedSo;
        var total    = critical + warning + info;

        return Ok(new { critical, warning, info, total });
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /api/alerts/live/stock
    // 재고 부족 목록 (CurrentStock <= SafetyStock 또는 <= 0)
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet("live/stock")]
    public async Task<IActionResult> LiveStock(CancellationToken ct)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync(new CommandDefinition(@"
            SELECT
                ItemCode,
                ISNULL(ItemName, ItemCode)  AS ItemName,
                CurrentStock,
                SafetyStock,
                ISNULL(Unit, 'EA')          AS Unit,
                WarehouseCode
            FROM dbo.Inventories
            WHERE CurrentStock <= SafetyStock
            ORDER BY CurrentStock ASC",
            cancellationToken: ct));

        return Ok(rows.Select(r => new {
            itemCode     = (string)r.ItemCode,
            itemName     = (string)r.ItemName,
            currentStock = (decimal)r.CurrentStock,
            safetyStock  = (decimal)r.SafetyStock,
            unit         = (string)r.Unit,
            warehouseCode= (string)r.WarehouseCode
        }));
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /api/alerts/live/duesoon
    // 납기 임박 작업지시 (오늘 ~ +3일 이내 완료 예정, 미완료)
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet("live/duesoon")]
    public async Task<IActionResult> LiveDueSoon(CancellationToken ct)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync(new CommandDefinition(@"
            SELECT
                WorkOrderNo,
                ISNULL(ItemName, ItemCode)  AS ItemName,
                PlannedEndDate
            FROM dbo.WorkOrders
            WHERE Status IN (0, 1, 2)
              AND PlannedEndDate <= CAST(DATEADD(day, 3, GETUTCDATE()) AS DATE)
              AND PlannedEndDate >= CAST(GETUTCDATE() AS DATE)
            ORDER BY PlannedEndDate ASC",
            cancellationToken: ct));

        return Ok(rows.Select(r => new {
            workOrderNo   = (string)r.WorkOrderNo,
            itemName      = (string)r.ItemName,
            plannedEndDate= (DateTime)r.PlannedEndDate
        }));
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /api/alerts/live/overdue
    // 작업지시 지연 (Status IN (1,2) AND PlannedEndDate < 오늘)
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet("live/overdue")]
    public async Task<IActionResult> LiveOverdue(CancellationToken ct)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync(new CommandDefinition(@"
            SELECT
                WorkOrderNo,
                ISNULL(ItemName, ItemCode)  AS ItemName,
                PlannedEndDate
            FROM dbo.WorkOrders
            WHERE Status IN (1, 2)
              AND PlannedEndDate < CAST(GETUTCDATE() AS DATE)
            ORDER BY PlannedEndDate ASC",
            cancellationToken: ct));

        return Ok(rows.Select(r => new {
            workOrderNo   = (string)r.WorkOrderNo,
            itemName      = (string)r.ItemName,
            plannedEndDate= (DateTime)r.PlannedEndDate
        }));
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /api/alerts/live/unconfirmed
    // 미확정 수주 (Status=0, OrderDate <= 오늘-3일)
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet("live/unconfirmed")]
    public async Task<IActionResult> LiveUnconfirmed(CancellationToken ct)
    {
        using var conn = _db.Create();
        var rows = await conn.QueryAsync(new CommandDefinition(@"
            SELECT
                SalesOrderNo,
                CustomerName,
                OrderDate
            FROM dbo.SalesOrders
            WHERE Status = 0
              AND OrderDate <= CAST(DATEADD(day, -3, GETUTCDATE()) AS DATE)
            ORDER BY OrderDate ASC",
            cancellationToken: ct));

        return Ok(rows.Select(r => new {
            salesOrderNo = (string)r.SalesOrderNo,
            customerName = (string)r.CustomerName,
            orderDate    = (DateTime)r.OrderDate
        }));
    }

    // ─────────────────────────────────────────────────────────────────────
    // 기존 Alerts 테이블 기반 액션 (정적 알림 관리용)
    // ─────────────────────────────────────────────────────────────────────

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

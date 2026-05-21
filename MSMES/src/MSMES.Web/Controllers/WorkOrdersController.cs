using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Domain.WorkOrder;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes =
    CookieAuthenticationDefaults.AuthenticationScheme + "," +
    JwtBearerDefaults.AuthenticationScheme)]
[Route("api/workorders")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderRepository _repo;

    public WorkOrdersController(IWorkOrderRepository repo) => _repo = repo;

    // GET /api/workorders?woNo=&item=&status=
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? woNo,
        [FromQuery] string? item,
        [FromQuery] string? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken ct = default)
    {
        var list = await _repo.ListFilteredAsync(woNo, item, status, skip, take, ct);
        return Ok(list.Select(w => ToDto(w)));
    }

    // GET /api/workorders/next-no
    [HttpGet("next-no")]
    public async Task<IActionResult> NextNo(CancellationToken ct)
    {
        var no = await _repo.NextNumberAsync(ct);
        return Ok(new { workOrderNo = no });
    }

    // GET /api/workorders/{no}
    [HttpGet("{no}")]
    public async Task<IActionResult> Get(string no, CancellationToken ct)
    {
        var w = await _repo.GetByNoAsync(no, ct);
        return w is null
            ? NotFound(new { message = $"작업지시 '{no}'를 찾을 수 없습니다." })
            : Ok(ToDto(w));
    }

    // POST /api/workorders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkOrderCreateRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var no = string.IsNullOrWhiteSpace(req.WorkOrderNo)
            ? await _repo.NextNumberAsync(ct)
            : req.WorkOrderNo.Trim();

        if (await _repo.GetByNoAsync(no, ct) is not null)
            return Conflict(new { message = $"작업지시번호 '{no}'가 이미 존재합니다." });

        var w = new WorkOrder
        {
            WorkOrderNo      = no,
            SalesOrderNo     = req.SalesOrderNo,
            ItemCode         = req.ItemCode,
            ItemName         = req.ItemName ?? string.Empty,
            ProcessCode      = req.ProcessCode,
            PlannedQuantity  = req.PlannedQuantity,
            ProducedQuantity = 0,
            StartDate        = req.StartDate,
            PlannedEndDate   = req.PlannedEndDate,
            Note             = req.Note,
            Status           = WorkOrderStatus.Planned,
            CreatedBy        = User.Identity?.Name,
            CreatedAt        = DateTime.UtcNow
        };

        await _repo.AddAsync(w, ct);
        return CreatedAtAction(nameof(Get), new { no = w.WorkOrderNo }, ToDto(w));
    }

    // PUT /api/workorders/{no}
    [HttpPut("{no}")]
    public async Task<IActionResult> Update(
        string no,
        [FromBody] WorkOrderUpdateRequest req,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var w = await _repo.GetByNoAsync(no, ct);
        if (w is null)
            return NotFound(new { message = $"작업지시 '{no}'를 찾을 수 없습니다." });

        w.SalesOrderNo    = req.SalesOrderNo;
        w.ItemCode        = req.ItemCode;
        w.ItemName        = req.ItemName ?? string.Empty;
        w.ProcessCode     = req.ProcessCode;
        w.PlannedQuantity = req.PlannedQuantity;
        w.StartDate       = req.StartDate;
        w.PlannedEndDate  = req.PlannedEndDate;
        w.Note            = req.Note;

        await _repo.UpdateAsync(w, ct);
        return Ok(ToDto(w));
    }

    // DELETE /api/workorders/{no}  — Planned 상태만 삭제
    [HttpDelete("{no}")]
    public async Task<IActionResult> Delete(string no, CancellationToken ct)
    {
        var w = await _repo.GetByNoAsync(no, ct);
        if (w is null)
            return NotFound(new { message = $"작업지시 '{no}'를 찾을 수 없습니다." });

        if (w.Status != WorkOrderStatus.Planned)
            return UnprocessableEntity(new { message = "계획(Planned) 상태의 작업지시만 삭제할 수 있습니다." });

        await _repo.DeleteAsync(no, ct);
        return NoContent();
    }

    // PATCH /api/workorders/{no}/status
    [HttpPatch("{no}/status")]
    public async Task<IActionResult> ChangeStatus(
        string no,
        [FromBody] WorkOrderStatusRequest req,
        CancellationToken ct)
    {
        var w = await _repo.GetByNoAsync(no, ct);
        if (w is null)
            return NotFound(new { message = $"작업지시 '{no}'를 찾을 수 없습니다." });

        try
        {
            switch (req.Action?.ToLowerInvariant())
            {
                case "release":  w.Release(); break;
                case "start":    w.Start(); break;
                case "complete": w.Complete(req.ProducedQty ?? w.PlannedQuantity); break;
                case "close":    w.Close(); break;
                case "cancel":   w.Cancel(); break;
                // POP 생산수량 입력: 상태 변경 없이 생산수량만 업데이트
                case "produce":
                    if (w.Status != WorkOrderStatus.InProgress)
                        throw new InvalidOperationException("진행중(InProgress) 상태에서만 생산수량을 입력할 수 있습니다.");
                    w.ProducedQuantity = req.ProducedQty ?? w.ProducedQuantity;
                    if (req.Note != null) w.Note = req.Note;
                    w.UpdatedAt = DateTime.UtcNow;
                    break;
                default:
                    return BadRequest(new
                    {
                        message = $"알 수 없는 액션: '{req.Action}'. 허용값: release, start, produce, complete, close, cancel"
                    });
            }
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }

        await _repo.UpdateAsync(w, ct);
        return Ok(ToDto(w));
    }

    // ── DTO helper ──────────────────────────────────────────
    private static object ToDto(WorkOrder w) => new
    {
        w.WorkOrderNo,
        w.SalesOrderNo,
        w.ItemCode,
        w.ItemName,
        w.ProcessCode,
        w.PlannedQuantity,
        w.ProducedQuantity,
        w.ProgressRate,
        StartDate      = w.StartDate.ToString("yyyy-MM-dd"),
        PlannedEndDate = w.PlannedEndDate.ToString("yyyy-MM-dd"),
        ActualEndDate  = w.ActualEndDate?.ToString("yyyy-MM-dd"),
        Status         = w.Status.ToString(),
        w.StatusName,
        w.StatusCss,
        w.Note,
        w.CreatedAt,
        w.CreatedBy
    };
}

// ── Request models ──────────────────────────────────────────
public sealed class WorkOrderCreateRequest
{
    public string? WorkOrderNo { get; set; }
    public string? SalesOrderNo { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string? ProcessCode { get; set; }
    public decimal PlannedQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string? Note { get; set; }
}

public sealed class WorkOrderUpdateRequest
{
    public string? SalesOrderNo { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string? ProcessCode { get; set; }
    public decimal PlannedQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string? Note { get; set; }
}

public sealed class WorkOrderStatusRequest
{
    public string? Action { get; set; }
    public decimal? ProducedQty { get; set; }
    public decimal? DefectQty { get; set; }
    public string? Note { get; set; }
}

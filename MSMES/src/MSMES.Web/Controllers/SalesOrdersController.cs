using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Domain.SalesOrder;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes =
    CookieAuthenticationDefaults.AuthenticationScheme + "," +
    JwtBearerDefaults.AuthenticationScheme)]
[Route("api/salesorders")]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderRepository _repo;

    public SalesOrdersController(ISalesOrderRepository repo) => _repo = repo;

    // GET /api/salesorders?orderNo=&customer=&status=&dateFrom=&dateTo=
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? orderNo,
        [FromQuery] string? customer,
        [FromQuery] string? status,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken ct = default)
    {
        DateTime? from = dateFrom is not null && DateTime.TryParse(dateFrom, out var fd) ? fd : null;
        DateTime? to   = dateTo   is not null && DateTime.TryParse(dateTo,   out var td) ? td : null;

        var list = await _repo.ListFilteredAsync(orderNo, customer, status, from, to, skip, take, ct);
        return Ok(list.Select(o => new
        {
            o.SalesOrderNo,
            o.CustomerCode,
            o.CustomerName,
            OrderDate    = o.OrderDate.ToString("yyyy-MM-dd"),
            DueDate      = o.DueDate.ToString("yyyy-MM-dd"),
            o.Status,
            o.StatusName,
            o.TotalAmount,
            o.Note
        }));
    }

    // GET /api/salesorders/next-no  (반드시 {no} 라우트보다 먼저 선언)
    [HttpGet("next-no")]
    public async Task<IActionResult> NextNo(CancellationToken ct)
    {
        var number = await _repo.NextNumberAsync(ct);
        return Ok(new { number });
    }

    // GET /api/salesorders/{no}
    [HttpGet("{no}")]
    public async Task<IActionResult> Get(string no, CancellationToken ct)
    {
        var so = await _repo.GetByNoAsync(no, ct);
        if (so is null) return NotFound(new { message = $"수주 {no}를 찾을 수 없습니다." });

        return Ok(new
        {
            so.SalesOrderNo,
            so.CustomerCode,
            so.CustomerName,
            OrderDate = so.OrderDate.ToString("yyyy-MM-dd"),
            DueDate   = so.DueDate.ToString("yyyy-MM-dd"),
            so.Status,
            so.StatusName,
            so.TotalAmount,
            so.Note,
            Items = so.Items.Select(i => new
            {
                i.ItemNo,
                i.ItemCode,
                i.ItemName,
                i.Quantity,
                i.UnitPrice,
                i.Amount
            })
        });
    }

    // POST /api/salesorders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveSalesOrderDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var no = await _repo.NextNumberAsync(ct);

        var order = new SalesOrder
        {
            SalesOrderNo = no,
            CustomerCode = dto.CustomerCode ?? string.Empty,
            CustomerName = dto.CustomerName ?? string.Empty,
            OrderDate    = dto.OrderDate,
            DueDate      = dto.DueDate,
            Status       = ParseStatus(dto.Status) ?? SalesOrderStatus.Draft,
            Note         = dto.Note,
            CreatedBy    = User.Identity?.Name ?? "system",
            CreatedAt    = DateTime.UtcNow
        };

        // 품목 처리
        int itemNo = 1;
        foreach (var item in dto.Items ?? Enumerable.Empty<SaveSalesOrderItemDto>())
        {
            order.Items.Add(new SalesOrderItem
            {
                SalesOrderNo = no,
                ItemNo       = itemNo++,
                ItemCode     = item.ItemCode ?? string.Empty,
                ItemName     = item.ItemName ?? string.Empty,
                Quantity     = item.Quantity,
                UnitPrice    = item.UnitPrice,
                CreatedAt    = order.CreatedAt,
                CreatedBy    = order.CreatedBy
            });
        }

        await _repo.AddAsync(order, ct);
        return CreatedAtAction(nameof(Get), new { no }, new { salesOrderNo = no });
    }

    // PUT /api/salesorders/{no}
    [HttpPut("{no}")]
    public async Task<IActionResult> Update(string no, [FromBody] SaveSalesOrderDto dto, CancellationToken ct)
    {
        var existing = await _repo.GetByNoAsync(no, ct);
        if (existing is null) return NotFound(new { message = $"수주 {no}를 찾을 수 없습니다." });

        existing.CustomerCode = dto.CustomerCode ?? existing.CustomerCode;
        existing.CustomerName = dto.CustomerName ?? existing.CustomerName;
        existing.OrderDate    = dto.OrderDate != default ? dto.OrderDate : existing.OrderDate;
        existing.DueDate      = dto.DueDate   != default ? dto.DueDate   : existing.DueDate;
        existing.Note         = dto.Note;

        if (ParseStatus(dto.Status) is { } s)
            existing.Status = s;

        await _repo.UpdateAsync(existing, ct);
        return NoContent();
    }

    // DELETE /api/salesorders/{no}
    [HttpDelete("{no}")]
    public async Task<IActionResult> Delete(string no, CancellationToken ct)
    {
        var existing = await _repo.GetByNoAsync(no, ct);
        if (existing is null) return NotFound(new { message = $"수주 {no}를 찾을 수 없습니다." });

        if (existing.Status != SalesOrderStatus.Draft)
            return BadRequest(new { message = "초안(Draft) 상태의 수주만 삭제할 수 있습니다." });

        await _repo.DeleteAsync(no, ct);
        return NoContent();
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────
    private static SalesOrderStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Enum.TryParse<SalesOrderStatus>(value, true, out var s) ? s : null;
    }
}

// ─── DTOs ───
public sealed class SaveSalesOrderDto
{
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public DateTime OrderDate   { get; set; }
    public DateTime DueDate     { get; set; }
    public string?  Status      { get; set; }
    public string?  Note        { get; set; }
    public List<SaveSalesOrderItemDto>? Items { get; set; }
}

public sealed class SaveSalesOrderItemDto
{
    public string?  ItemCode  { get; set; }
    public string?  ItemName  { get; set; }
    public decimal  Quantity  { get; set; }
    public decimal  UnitPrice { get; set; }
}

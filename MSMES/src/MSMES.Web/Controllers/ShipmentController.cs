using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Shipment;
using MSMES.Domain.Shipment;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/shipments")]
public class ShipmentController : ControllerBase
{
    private readonly IShipmentRepository _repo;
    private readonly CreateShipmentHandler _create;

    public ShipmentController(IShipmentRepository repo, CreateShipmentHandler create)
    {
        _repo = repo;
        _create = create;
    }

    /// <summary>전체 출하 목록</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct = default)
    {
        var list = await _repo.ListAllAsync(ct);
        var result = list.Select(s => new
        {
            s.ShipmentNo,
            s.SalesOrderNo,
            s.ShipmentDate,
            s.DeliveryAddress,
            Status = s.Status.ToString(),
            StatusCode = (int)s.Status,
            ItemCount = s.Items.Count
        });
        return Ok(result);
    }

    /// <summary>출하 단건 조회</summary>
    [HttpGet("{no}")]
    public async Task<IActionResult> Get(string no, CancellationToken ct = default)
    {
        var s = await _repo.GetByNoAsync(no, ct);
        if (s is null) return NotFound();
        return Ok(new
        {
            s.ShipmentNo,
            s.SalesOrderNo,
            s.ShipmentDate,
            s.DeliveryAddress,
            Status = s.Status.ToString(),
            StatusCode = (int)s.Status,
            Items = s.Items.Select(i => new
            {
                i.ItemNo,
                i.LotNo,
                i.ItemCode,
                i.ShipQuantity
            })
        });
    }

    /// <summary>출하 품목 목록</summary>
    [HttpGet("{no}/items")]
    public async Task<IActionResult> GetItems(string no, CancellationToken ct = default)
    {
        var items = await _repo.GetItemsByShipmentNoAsync(no, ct);
        return Ok(items.Select(i => new
        {
            i.ItemNo,
            i.ShipmentNo,
            i.LotNo,
            i.ItemCode,
            i.ShipQuantity
        }));
    }

    /// <summary>출하 등록</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userName = User.Identity?.Name ?? "system";
        var cmd = new CreateShipmentCommand(
            req.SalesOrderNo,
            req.ShipmentDate ?? DateTime.Today,
            req.DeliveryAddress,
            req.Items.Select(i => new CreateShipmentItemDto(i.LotNo, i.ItemCode, i.ShipQuantity)).ToList(),
            userName);

        try
        {
            var no = await _create.HandleAsync(cmd, ct);
            return Ok(new { ShipmentNo = no, Success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>상태 변경</summary>
    [HttpPatch("{no}/status")]
    public async Task<IActionResult> ChangeStatus(string no, [FromBody] ChangeStatusRequest req, CancellationToken ct = default)
    {
        var s = await _repo.GetByNoAsync(no, ct);
        if (s is null) return NotFound();

        if (!Enum.TryParse<ShipmentStatus>(req.Status, ignoreCase: true, out var newStatus))
            return BadRequest(new { Message = $"유효하지 않은 상태값: {req.Status}" });

        s.Status = newStatus;
        s.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(s, ct);

        return Ok(new { ShipmentNo = no, Status = newStatus.ToString(), Success = true });
    }
}

public sealed record CreateShipmentItemRequest(string LotNo, string ItemCode, decimal ShipQuantity);

public sealed record CreateShipmentRequest(
    string SalesOrderNo,
    string DeliveryAddress,
    DateTime? ShipmentDate,
    string? AssignedTo,
    IReadOnlyList<CreateShipmentItemRequest> Items);

public sealed record ChangeStatusRequest(string Status);

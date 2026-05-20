using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.PurchaseOrder;
using MSMES.Domain.PurchaseOrder;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/purchase-orders")]
public class PurchaseOrderController : ControllerBase
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly CreatePurchaseOrderHandler _create;

    public PurchaseOrderController(IPurchaseOrderRepository repo, CreatePurchaseOrderHandler create)
    {
        _repo = repo;
        _create = create;
    }

    /// <summary>전체 발주 목록</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct = default)
    {
        var list = await _repo.ListAllAsync(ct);
        var result = list.Select(po => new
        {
            po.PurchaseOrderNo,
            po.SupplierCode,
            po.SupplierName,
            OrderDate = po.OrderDate.ToString("yyyy-MM-dd"),
            DueDate = po.DueDate.ToString("yyyy-MM-dd"),
            Status = po.Status.ToString(),
            StatusCode = (int)po.Status,
            po.AssignedTo,
            po.Note,
            ItemCount = po.Items.Count,
            TotalAmount = po.TotalAmount
        });
        return Ok(result);
    }

    /// <summary>발주 단건 조회 (품목 포함)</summary>
    [HttpGet("{no}")]
    public async Task<IActionResult> Get(string no, CancellationToken ct = default)
    {
        var po = await _repo.GetWithItemsAsync(no, ct);
        if (po is null) return NotFound();
        return Ok(new
        {
            po.PurchaseOrderNo,
            po.SupplierCode,
            po.SupplierName,
            OrderDate = po.OrderDate.ToString("yyyy-MM-dd"),
            DueDate = po.DueDate.ToString("yyyy-MM-dd"),
            Status = po.Status.ToString(),
            StatusCode = (int)po.Status,
            po.AssignedTo,
            po.Note,
            TotalAmount = po.TotalAmount,
            Items = po.Items.Select(i => new
            {
                i.ItemNo,
                i.ItemCode,
                i.ItemName,
                i.OrderQuantity,
                i.UnitPrice,
                Amount = i.Amount,
                i.ReceivedQuantity,
                ReceiveRate = i.ReceiveRate
            })
        });
    }

    /// <summary>발주 등록</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userName = User.Identity?.Name ?? "system";
        var cmd = new CreatePurchaseOrderCommand(
            req.SupplierCode,
            req.SupplierName,
            req.OrderDate ?? DateTime.Today,
            req.DueDate,
            req.Items.Select(i => new CreatePurchaseOrderItemDto(i.ItemCode, i.ItemName, i.OrderQuantity, i.UnitPrice)).ToList(),
            userName,
            req.AssignedTo,
            req.Note);

        try
        {
            var no = await _create.HandleAsync(cmd, ct);
            return Ok(new { PurchaseOrderNo = no, Success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>상태 변경</summary>
    [HttpPatch("{no}/status")]
    public async Task<IActionResult> ChangeStatus(string no, [FromBody] ChangePurchaseOrderStatusRequest req, CancellationToken ct = default)
    {
        var po = await _repo.GetByNoAsync(no, ct);
        if (po is null) return NotFound();

        if (!Enum.TryParse<PurchaseOrderStatus>(req.Status, ignoreCase: true, out var newStatus))
            return BadRequest(new { Message = $"유효하지 않은 상태값: {req.Status}" });

        po.Status = newStatus;
        po.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(po, ct);

        return Ok(new { PurchaseOrderNo = no, Status = newStatus.ToString(), Success = true });
    }
}

public sealed record CreatePurchaseOrderItemRequest(
    string ItemCode,
    string ItemName,
    decimal OrderQuantity,
    decimal UnitPrice);

public sealed record CreatePurchaseOrderRequest(
    string SupplierCode,
    string SupplierName,
    DateTime? OrderDate,
    DateTime DueDate,
    string? AssignedTo,
    string? Note,
    IReadOnlyList<CreatePurchaseOrderItemRequest> Items);

public sealed record ChangePurchaseOrderStatusRequest(string Status);

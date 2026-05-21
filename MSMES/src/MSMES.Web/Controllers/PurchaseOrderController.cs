using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.PurchaseOrder;
using MSMES.Domain.PurchaseOrder;
using MSMES.Infrastructure.Repositories;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes =
    CookieAuthenticationDefaults.AuthenticationScheme + "," +
    JwtBearerDefaults.AuthenticationScheme)]
[Route("api/purchaseorders")]
public class PurchaseOrderController : ControllerBase
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly CreatePurchaseOrderHandler _create;
    private readonly SqlPurchaseOrderRepository _sqlRepo;

    public PurchaseOrderController(IPurchaseOrderRepository repo, CreatePurchaseOrderHandler create)
    {
        _repo = repo;
        _create = create;
        _sqlRepo = (SqlPurchaseOrderRepository)repo;
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

    /// <summary>다음 발주번호 조회</summary>
    [HttpGet("next-no")]
    public async Task<IActionResult> NextNo(CancellationToken ct = default)
    {
        var no = await _repo.NextNumberAsync(ct);
        return Ok(new { Number = no });
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

    /// <summary>발주 수정 (헤더 + 품목 재삽입, Draft/Issued만)</summary>
    [HttpPut("{no}")]
    public async Task<IActionResult> Update(string no, [FromBody] UpdatePurchaseOrderRequest req, CancellationToken ct = default)
    {
        var po = await _repo.GetWithItemsAsync(no, ct);
        if (po is null) return NotFound(new { Message = $"발주번호 {no}를 찾을 수 없습니다." });

        if (po.Status != PurchaseOrderStatus.Draft && po.Status != PurchaseOrderStatus.Issued)
            return BadRequest(new { Message = "Draft 또는 Issued 상태의 발주만 수정할 수 있습니다." });

        po.SupplierCode = req.SupplierCode;
        po.SupplierName = req.SupplierName;
        po.OrderDate    = req.OrderDate ?? po.OrderDate;
        po.DueDate      = req.DueDate;
        po.AssignedTo   = req.AssignedTo;
        po.Note         = req.Note;
        po.UpdatedAt    = DateTime.UtcNow;
        po.Items = req.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.ItemCode) && i.OrderQuantity > 0)
            .Select(i => new PurchaseOrderItem
            {
                PurchaseOrderNo = no,
                ItemCode        = i.ItemCode,
                ItemName        = i.ItemName,
                OrderQuantity   = i.OrderQuantity,
                UnitPrice       = i.UnitPrice
            }).ToList();

        try
        {
            await _sqlRepo.UpdateWithItemsAsync(po, ct);
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

        po.Status    = newStatus;
        po.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(po, ct);

        return Ok(new { PurchaseOrderNo = no, Status = newStatus.ToString(), Success = true });
    }

    /// <summary>발주 삭제 (Draft만)</summary>
    [HttpDelete("{no}")]
    public async Task<IActionResult> Delete(string no, CancellationToken ct = default)
    {
        var po = await _repo.GetByNoAsync(no, ct);
        if (po is null) return NotFound(new { Message = $"발주번호 {no}를 찾을 수 없습니다." });

        if (po.Status != PurchaseOrderStatus.Draft)
            return BadRequest(new { Message = "Draft 상태의 발주만 삭제할 수 있습니다." });

        await _repo.DeleteAsync(no, ct);
        return Ok(new { PurchaseOrderNo = no, Success = true });
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────────

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

public sealed record UpdatePurchaseOrderItemRequest(
    string ItemCode,
    string ItemName,
    decimal OrderQuantity,
    decimal UnitPrice);

public sealed record UpdatePurchaseOrderRequest(
    string SupplierCode,
    string SupplierName,
    DateTime? OrderDate,
    DateTime DueDate,
    string? AssignedTo,
    string? Note,
    IReadOnlyList<UpdatePurchaseOrderItemRequest> Items);

public sealed record ChangePurchaseOrderStatusRequest(string Status);

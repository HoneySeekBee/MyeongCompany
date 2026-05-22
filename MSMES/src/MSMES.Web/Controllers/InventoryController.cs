using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Inventory;
using MSMES.Domain.Inventory;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly GetInventoryStatusHandler _status;
    private readonly CreateInventoryTransactionHandler _transaction;
    private readonly IInventoryRepository _repo;

    public InventoryController(
        GetInventoryStatusHandler status,
        CreateInventoryTransactionHandler transaction,
        IInventoryRepository repo)
    {
        _status = status;
        _transaction = transaction;
        _repo = repo;
    }

    /// <summary>전체 재고 현황</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
        => Ok(await _status.HandleAsync(new GetInventoryStatusQuery(null, skip, take), ct));

    /// <summary>부족재고(LowStock + OutOfStock) 목록</summary>
    [HttpGet("lowstock")]
    public async Task<IActionResult> LowStock(CancellationToken ct = default)
    {
        var low = await _status.HandleAsync(new GetInventoryStatusQuery(InventoryStatus.LowStock), ct);
        var outOfStock = await _status.HandleAsync(new GetInventoryStatusQuery(InventoryStatus.OutOfStock), ct);
        var combined = low.Concat(outOfStock).OrderBy(i => i.ItemCode).ToList();
        return Ok(combined);
    }

    /// <summary>입출고 이력 조회</summary>
    [HttpGet("{itemCode}/history")]
    public async Task<IActionResult> History(
        string itemCode,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var list = await _repo.GetTransactionsAsync(itemCode, from, to, ct);
        return Ok(list);
    }

    /// <summary>최근 입출고 이력 (전체 품목)</summary>
    [HttpGet("transactions/recent")]
    public async Task<IActionResult> RecentTransactions(
        [FromQuery] int count = 50,
        CancellationToken ct = default)
    {
        var list = await _repo.GetRecentTransactionsAsync(count, ct);
        return Ok(list);
    }

    /// <summary>입고/출고/조정 처리</summary>
    [HttpPost("transaction")]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] CreateInventoryTransactionCommand cmd,
        CancellationToken ct = default)
    {
        try
        {
            var txId = await _transaction.HandleAsync(cmd, ct);
            return Ok(new { TransactionId = txId, Success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>트랜잭션 목록 (페이징)</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions(
        [FromQuery] string? itemCode,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken ct = default)
    {
        var list = await _repo.ListTransactionsAsync(itemCode, skip, take, ct);
        return Ok(list);
    }

    /// <summary>재고 조정 (입고/출고/조정 통합 — StockBefore/After 자동 추적)</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustStockRequest req,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ItemCode))
            return BadRequest(new { Success = false, Message = "품목코드를 입력하세요." });
        if (string.IsNullOrWhiteSpace(req.WarehouseCode))
            return BadRequest(new { Success = false, Message = "창고코드를 입력하세요." });
        if (req.Quantity <= 0)
            return BadRequest(new { Success = false, Message = "수량은 0보다 커야 합니다." });

        try
        {
            var createdBy = User.FindFirst("UserName")?.Value
                         ?? User.Identity?.Name
                         ?? "Unknown";

            await _repo.AdjustStockAsync(
                req.ItemCode, req.WarehouseCode,
                req.Quantity, req.TransactionType,
                req.Reason ?? string.Empty,
                req.Reference,
                createdBy, ct);

            return Ok(new { Success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}

public sealed record AdjustStockRequest(
    string ItemCode,
    string WarehouseCode,
    MSMES.Domain.Inventory.InventoryTransactionType TransactionType,
    decimal Quantity,
    string? Reason,
    string? Reference);

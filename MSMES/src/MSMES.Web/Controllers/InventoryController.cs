using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Inventory;
using MSMES.Domain.Inventory;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize]
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

    /// <summary>입고/출고/조정 처리</summary>
    [HttpPost("transaction")]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] CreateInventoryTransactionCommand cmd,
        CancellationToken ct = default)
    {
        var txId = await _transaction.HandleAsync(cmd, ct);
        return Ok(new { TransactionId = txId });
    }
}

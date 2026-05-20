using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.LotManagement;
using MSMES.Domain.LotManagement;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/lots")]
public class LotController : ControllerBase
{
    private readonly ListAllLotsHandler _listAll;
    private readonly CreateLotHandler _create;
    private readonly GetLotHistoryHandler _history;
    private readonly UpdateLotStatusHandler _updateStatus;

    public LotController(
        ListAllLotsHandler listAll,
        CreateLotHandler create,
        GetLotHistoryHandler history,
        UpdateLotStatusHandler updateStatus)
    {
        _listAll = listAll;
        _create = create;
        _history = history;
        _updateStatus = updateStatus;
    }

    /// <summary>전체 LOT 목록 조회</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var lots = await _listAll.HandleAsync(ct);
        return Ok(lots);
    }

    /// <summary>LOT 이력 조회</summary>
    [HttpGet("{lotNo}/history")]
    public async Task<IActionResult> GetHistory(string lotNo, CancellationToken ct = default)
    {
        var histories = await _history.HandleAsync(new GetLotHistoryQuery(lotNo), ct);
        return Ok(histories);
    }

    /// <summary>LOT 등록</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLotRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var cmd = new CreateLotCommand(
            req.ItemCode,
            req.ItemName ?? string.Empty,
            req.WorkOrderNo ?? string.Empty,
            req.ProducedQuantity,
            req.DefectQuantity,
            req.ProductionDate,
            req.CreatedBy ?? User.Identity?.Name ?? "system"
        );
        var lotNo = await _create.HandleAsync(cmd, ct);
        return Ok(new { lotNo });
    }

    /// <summary>LOT 상태 변경</summary>
    [HttpPatch("{lotNo}/status")]
    public async Task<IActionResult> UpdateStatus(
        string lotNo,
        [FromBody] UpdateStatusRequest req,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!Enum.TryParse<LotStatus>(req.Status, true, out var newStatus))
            return BadRequest(new { error = $"유효하지 않은 상태값: {req.Status}" });

        try
        {
            var cmd = new UpdateLotStatusCommand(lotNo, newStatus, req.Operator ?? User.Identity?.Name ?? "system");
            await _updateStatus.HandleAsync(cmd, ct);
            return Ok(new { lotNo, status = newStatus.ToString() });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

public sealed record CreateLotRequest(
    string ItemCode,
    string? ItemName,
    string? WorkOrderNo,
    decimal ProducedQuantity,
    decimal DefectQuantity,
    DateTime ProductionDate,
    string? CreatedBy
);

public sealed record UpdateStatusRequest(string Status, string? Operator);

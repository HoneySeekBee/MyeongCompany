using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Process;
using MSMES.Domain.Process;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/process")]
public class ProcessController : ControllerBase
{
    private readonly IProcessRepository _repo;
    private readonly CreateProductionResultHandler _createResult;
    private readonly GetProductionResultHandler _getResult;

    public ProcessController(
        IProcessRepository repo,
        CreateProductionResultHandler createResult,
        GetProductionResultHandler getResult)
    {
        _repo = repo;
        _createResult = createResult;
        _getResult = getResult;
    }

    /// <summary>공정 정의 목록</summary>
    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions(CancellationToken ct = default)
    {
        var list = await _repo.ListProcessesAsync(ct);
        return Ok(list);
    }

    /// <summary>생산실적 등록</summary>
    [HttpPost("results")]
    public async Task<IActionResult> CreateResult(
        [FromBody] CreateProductionResultCommand cmd,
        CancellationToken ct = default)
    {
        var no = await _createResult.HandleAsync(cmd, ct);
        return CreatedAtAction(nameof(GetResultsByWorkOrder),
            new { workOrderNo = cmd.WorkOrderNo },
            new { resultNo = no });
    }

    /// <summary>작업지시별 생산실적 조회</summary>
    [HttpGet("results/{workOrderNo}")]
    public async Task<IActionResult> GetResultsByWorkOrder(
        string workOrderNo,
        CancellationToken ct = default)
    {
        var results = await _repo.ListResultsByWorkOrderAsync(workOrderNo, ct);
        return Ok(results);
    }

    /// <summary>일별 생산 집계 (기본: 오늘)</summary>
    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailySummary(
        [FromQuery] DateTime? date,
        CancellationToken ct = default)
    {
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;
        var from = targetDate;
        var to = targetDate.AddDays(1);
        var summary = await _getResult.HandleAsync(new GetProductionResultQuery(from, to), ct);
        return Ok(summary);
    }
}

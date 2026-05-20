using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Process;
using MSMES.Domain.Process;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

    /// <summary>공정 정의 등록</summary>
    [HttpPost("definitions")]
    public async Task<IActionResult> CreateDefinition(
        [FromBody] CreateProcessDefinitionRequest req,
        CancellationToken ct = default)
    {
        var existing = await _repo.GetProcessAsync(req.ProcessCode, ct);
        if (existing is not null)
            return Conflict(new { message = $"이미 존재하는 공정코드입니다: {req.ProcessCode}" });

        var definition = new ProcessDefinition
        {
            ProcessCode         = req.ProcessCode,
            ProcessName         = req.ProcessName,
            ProcessOrder        = req.ProcessOrder,
            StandardTimeMinutes = req.StandardTimeMinutes,
            EquipmentType       = req.EquipmentType ?? string.Empty,
            IsActive            = true,
            CreatedAt           = DateTime.UtcNow,
            CreatedBy           = User.Identity?.Name ?? "system"
        };
        await _repo.AddProcessAsync(definition, ct);
        return CreatedAtAction(nameof(GetDefinitions), new { }, new { processCode = definition.ProcessCode });
    }

    /// <summary>공정 정의 수정</summary>
    [HttpPut("definitions/{processCode}")]
    public async Task<IActionResult> UpdateDefinition(
        string processCode,
        [FromBody] CreateProcessDefinitionRequest req,
        CancellationToken ct = default)
    {
        var existing = await _repo.GetProcessAsync(processCode, ct);
        if (existing is null)
            return NotFound(new { message = $"공정을 찾을 수 없습니다: {processCode}" });

        existing.ProcessName         = req.ProcessName;
        existing.ProcessOrder        = req.ProcessOrder;
        existing.StandardTimeMinutes = req.StandardTimeMinutes;
        existing.EquipmentType       = req.EquipmentType ?? string.Empty;
        await _repo.UpdateProcessAsync(existing, ct);
        return NoContent();
    }

    /// <summary>공정 정의 삭제 (비활성화)</summary>
    [HttpDelete("definitions/{processCode}")]
    public async Task<IActionResult> DeleteDefinition(
        string processCode,
        CancellationToken ct = default)
    {
        var existing = await _repo.GetProcessAsync(processCode, ct);
        if (existing is null)
            return NotFound(new { message = $"공정을 찾을 수 없습니다: {processCode}" });

        existing.IsActive = false;
        await _repo.UpdateProcessAsync(existing, ct);
        return NoContent();
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

public sealed record CreateProcessDefinitionRequest(
    string ProcessCode,
    string ProcessName,
    int ProcessOrder,
    decimal StandardTimeMinutes,
    string? EquipmentType);

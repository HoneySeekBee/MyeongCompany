using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Quality;
using MSMES.Domain.Quality;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/quality")]
public class QualityController : ControllerBase
{
    private readonly CreateQualityInspectionHandler _create;
    private readonly GetQualityReportHandler _report;
    private readonly IQualityRepository _repo;

    public QualityController(
        CreateQualityInspectionHandler create,
        GetQualityReportHandler report,
        IQualityRepository repo)
    {
        _create = create;
        _report = report;
        _repo = repo;
    }

    /// <summary>검사 목록</summary>
    [HttpGet("inspections")]
    public async Task<IActionResult> List(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
        => Ok(await _repo.ListAsync(skip, take, ct));

    /// <summary>검사대기 목록</summary>
    [HttpGet("inspections/pending")]
    public async Task<IActionResult> Pending(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var all = await _repo.ListAsync(skip, take, ct);
        var pending = all.Where(i => i.Result == QualityStatus.Pending).ToList();
        return Ok(pending);
    }

    /// <summary>검사 결과 등록</summary>
    [HttpPost("inspections")]
    public async Task<IActionResult> Create(
        [FromBody] CreateQualityInspectionCommand cmd,
        CancellationToken ct = default)
    {
        var no = await _create.HandleAsync(cmd, ct);
        return CreatedAtAction(nameof(GetByNo), new { no }, new { InspectionNo = no });
    }

    /// <summary>검사번호로 단건 조회 (CreateAtAction 연결용)</summary>
    [HttpGet("inspections/{no}")]
    public async Task<IActionResult> GetByNo(string no, CancellationToken ct = default)
    {
        var inspection = await _repo.GetByNoAsync(no, ct);
        return inspection is null ? NotFound() : Ok(inspection);
    }

    /// <summary>불량 통계 (기간 쿼리 파라미터: from, to)</summary>
    [HttpGet("report")]
    public async Task<IActionResult> Report(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct = default)
        => Ok(await _report.HandleAsync(new GetQualityReportQuery(from, to), ct));
}

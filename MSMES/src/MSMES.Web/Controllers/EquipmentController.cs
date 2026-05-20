using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Equipment;
using MSMES.Domain.Equipment;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/equipment")]
public class EquipmentController : ControllerBase
{
    private readonly GetEquipmentStatusHandler _getStatus;
    private readonly CreateMaintenanceHandler _createMaintenance;
    private readonly IEquipmentRepository _repo;

    public EquipmentController(
        GetEquipmentStatusHandler getStatus,
        CreateMaintenanceHandler createMaintenance,
        IEquipmentRepository repo)
    {
        _getStatus = getStatus;
        _createMaintenance = createMaintenance;
        _repo = repo;
    }

    /// <summary>설비 전체 목록 또는 상태별 조회</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] EquipmentStatus? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var list = await _getStatus.HandleAsync(new GetEquipmentStatusQuery(status, skip, take), ct);
        return Ok(list);
    }

    /// <summary>설비 등록</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEquipmentRequest req,
        CancellationToken ct = default)
    {
        var existing = await _repo.GetByCodeAsync(req.EquipmentCode, ct);
        if (existing is not null)
            return Conflict(new { message = $"이미 존재하는 설비코드입니다: {req.EquipmentCode}" });

        var equipment = new Domain.Equipment.Equipment
        {
            EquipmentCode      = req.EquipmentCode,
            EquipmentName      = req.EquipmentName,
            EquipmentType      = req.EquipmentType ?? string.Empty,
            Location           = req.Location ?? string.Empty,
            Status             = req.Status,
            LastInspectionDate = req.LastInspectionDate,
            NextInspectionDate = req.NextInspectionDate,
            CreatedAt          = DateTime.UtcNow,
            CreatedBy          = User.Identity?.Name ?? "system"
        };
        await _repo.AddAsync(equipment, ct);
        return CreatedAtAction(nameof(List), new { }, new { equipmentCode = equipment.EquipmentCode });
    }

    /// <summary>점검 예정일 7일 이내 설비 목록</summary>
    [HttpGet("maintenance-due")]
    public async Task<IActionResult> MaintenanceDue(CancellationToken ct = default)
    {
        var list = await _repo.GetMaintenanceDueAsync(ct);
        return Ok(list);
    }

    /// <summary>설비 상태 변경 (PATCH /api/equipment/{code}/status)</summary>
    [HttpPatch("{code}/status")]
    public async Task<IActionResult> UpdateStatus(
        string code,
        [FromBody] UpdateEquipmentStatusRequest request,
        CancellationToken ct = default)
    {
        var equipment = await _repo.GetByCodeAsync(code, ct);
        if (equipment is null)
            return NotFound(new { message = $"설비를 찾을 수 없습니다: {code}" });

        await _repo.UpdateStatusAsync(code, request.Status, ct);
        return NoContent();
    }

    /// <summary>점검 이력 등록</summary>
    [HttpPost("{code}/maintenance")]
    public async Task<IActionResult> CreateMaintenance(
        string code,
        [FromBody] CreateMaintenanceCommand cmd,
        CancellationToken ct = default)
    {
        var finalCmd = cmd with { EquipmentCode = code };
        var no = await _createMaintenance.HandleAsync(finalCmd, ct);
        return CreatedAtAction(nameof(List), new { }, new { maintenanceNo = no });
    }
}

public sealed record UpdateEquipmentStatusRequest(EquipmentStatus Status);

public sealed record CreateEquipmentRequest(
    string EquipmentCode,
    string EquipmentName,
    string? EquipmentType,
    string? Location,
    EquipmentStatus Status,
    DateTime? LastInspectionDate,
    DateTime? NextInspectionDate);

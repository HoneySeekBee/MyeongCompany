using MSMES.Domain.Equipment;

namespace MSMES.Application.Equipment;

public sealed record GetEquipmentStatusQuery(EquipmentStatus? Status, int Skip = 0, int Take = 50);

public sealed class GetEquipmentStatusHandler
{
    private readonly IEquipmentRepository _repo;
    public GetEquipmentStatusHandler(IEquipmentRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Domain.Equipment.Equipment>> HandleAsync(GetEquipmentStatusQuery q, CancellationToken ct = default)
        => q.Status.HasValue
            ? _repo.ListByStatusAsync(q.Status.Value, ct)
            : _repo.ListAsync(q.Skip, q.Take, ct);
}

public sealed record CreateMaintenanceCommand(
    string EquipmentCode,
    string MaintenanceType,
    string Description,
    string Operator,
    DateTime? NextDueDate,
    string CreatedBy);

public sealed class CreateMaintenanceHandler
{
    private readonly IEquipmentRepository _repo;
    public CreateMaintenanceHandler(IEquipmentRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreateMaintenanceCommand cmd, CancellationToken ct = default)
    {
        var equipment = await _repo.GetByCodeAsync(cmd.EquipmentCode, ct)
                        ?? throw new InvalidOperationException($"Equipment not found: {cmd.EquipmentCode}");

        var no = await _repo.NextMaintenanceNoAsync(ct);
        var maintenance = new EquipmentMaintenance
        {
            MaintenanceNo = no,
            EquipmentCode = cmd.EquipmentCode,
            MaintenanceType = cmd.MaintenanceType,
            Description = cmd.Description,
            Operator = cmd.Operator,
            MaintenanceDate = DateTime.UtcNow,
            NextDueDate = cmd.NextDueDate,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddMaintenanceAsync(maintenance, ct);

        equipment.LastInspectionDate = maintenance.MaintenanceDate;
        equipment.NextInspectionDate = cmd.NextDueDate;
        await _repo.UpdateAsync(equipment, ct);

        return no;
    }
}

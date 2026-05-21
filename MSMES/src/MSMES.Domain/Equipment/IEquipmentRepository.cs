namespace MSMES.Domain.Equipment;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> ListByStatusAsync(EquipmentStatus status, CancellationToken ct = default);
    Task AddAsync(Equipment equipment, CancellationToken ct = default);
    Task UpdateAsync(Equipment equipment, CancellationToken ct = default);

    Task<string> NextMaintenanceNoAsync(CancellationToken ct = default);
    Task AddMaintenanceAsync(EquipmentMaintenance maintenance, CancellationToken ct = default);
    Task<IReadOnlyList<EquipmentMaintenance>> GetMaintenanceHistoryAsync(string equipmentCode, CancellationToken ct = default);

    Task<(int Running, int Stopped, int Maintenance, int Breakdown)> GetStatusSummaryAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Equipment>> GetMaintenanceDueAsync(CancellationToken ct = default);
    Task UpdateStatusAsync(string equipmentCode, EquipmentStatus status, CancellationToken ct = default);

    Task<IReadOnlyList<OeeRecord>> GetOeeRecordsAsync(DateTime from, DateTime to, CancellationToken ct = default);

    Task<IReadOnlyList<PmSchedule>> GetPmSchedulesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PmRecord>>   GetPmRecordsAsync(int? equipmentId, CancellationToken ct = default);
    Task<int>                       CreatePmRecordAsync(PmRecord record, CancellationToken ct = default);
    Task                            UpdatePmScheduleNextDateAsync(int scheduleId, DateTime nextDate, CancellationToken ct = default);
}

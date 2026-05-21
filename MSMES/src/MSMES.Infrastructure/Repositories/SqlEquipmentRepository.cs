using Dapper;
using MSMES.Domain.Equipment;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlEquipmentRepository : IEquipmentRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SqlEquipmentRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<Equipment?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Equipment>(new CommandDefinition(
            "SELECT * FROM dbo.Equipments WHERE EquipmentCode = @code",
            new { code }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Equipment>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT * FROM dbo.Equipments
            ORDER BY EquipmentCode
            OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
        var rows = await conn.QueryAsync<Equipment>(new CommandDefinition(sql, new { skip, take }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<Equipment>> ListByStatusAsync(EquipmentStatus status, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = "SELECT * FROM dbo.Equipments WHERE Status = @status ORDER BY EquipmentCode";
        var rows = await conn.QueryAsync<Equipment>(new CommandDefinition(sql, new { status = (int)status }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(Equipment equipment, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            INSERT INTO dbo.Equipments
                (EquipmentCode, EquipmentName, EquipmentType, Location, Status,
                 LastInspectionDate, NextInspectionDate, CreatedAt, CreatedBy)
            VALUES
                (@EquipmentCode, @EquipmentName, @EquipmentType, @Location, @Status,
                 @LastInspectionDate, @NextInspectionDate, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, equipment, cancellationToken: ct));
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            UPDATE dbo.Equipments SET
                EquipmentName       = @EquipmentName,
                EquipmentType       = @EquipmentType,
                Location            = @Location,
                Status              = @Status,
                LastInspectionDate  = @LastInspectionDate,
                NextInspectionDate  = @NextInspectionDate,
                UpdatedAt           = SYSUTCDATETIME()
            WHERE EquipmentCode = @EquipmentCode";
        await conn.ExecuteAsync(new CommandDefinition(sql, equipment, cancellationToken: ct));
    }

    public Task<string> NextMaintenanceNoAsync(CancellationToken ct = default)
        => NumberSequence.NextAsync(_factory, "MNT", "MNT", ct);

    public async Task AddMaintenanceAsync(EquipmentMaintenance maintenance, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            INSERT INTO dbo.EquipmentMaintenances
                (MaintenanceNo, EquipmentCode, MaintenanceType, Description,
                 MaintenanceDate, Operator, NextDueDate, CreatedAt, CreatedBy)
            VALUES
                (@MaintenanceNo, @EquipmentCode, @MaintenanceType, @Description,
                 @MaintenanceDate, @Operator, @NextDueDate, @CreatedAt, @CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, maintenance, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<EquipmentMaintenance>> GetMaintenanceHistoryAsync(string equipmentCode, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT * FROM dbo.EquipmentMaintenances
            WHERE EquipmentCode = @code
            ORDER BY MaintenanceDate DESC";
        var rows = await conn.QueryAsync<EquipmentMaintenance>(new CommandDefinition(sql, new { code = equipmentCode }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<(int Running, int Stopped, int Maintenance, int Breakdown)> GetStatusSummaryAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT
                SUM(CASE WHEN Status = 0 THEN 1 ELSE 0 END) AS Running,
                SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS Stopped,
                SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) AS Maintenance,
                SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS Breakdown
            FROM dbo.Equipments";
        var row = await conn.QuerySingleAsync(new CommandDefinition(sql, cancellationToken: ct));
        return ((int)(row.Running ?? 0), (int)(row.Stopped ?? 0),
                (int)(row.Maintenance ?? 0), (int)(row.Breakdown ?? 0));
    }

    // 점검 예정일 7일 이내 설비
    public async Task<IReadOnlyList<Equipment>> GetMaintenanceDueAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            SELECT * FROM dbo.Equipments
            WHERE NextInspectionDate IS NOT NULL
              AND NextInspectionDate <= DATEADD(DAY, 7, SYSUTCDATETIME())
              AND NextInspectionDate >= SYSUTCDATETIME()
            ORDER BY NextInspectionDate";
        var rows = await conn.QueryAsync<Equipment>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    // 상태 변경 (단순 상태 업데이트)
    public async Task UpdateStatusAsync(string equipmentCode, EquipmentStatus status, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            UPDATE dbo.Equipments
            SET Status = @status, UpdatedAt = SYSUTCDATETIME()
            WHERE EquipmentCode = @code";
        await conn.ExecuteAsync(new CommandDefinition(sql, new { status = (int)status, code = equipmentCode }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<OeeRecord>> GetOeeRecordsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<OeeRecord>(new CommandDefinition(
            "SELECT * FROM OeeRecords WHERE RecordDate >= @from AND RecordDate < @to ORDER BY RecordDate, EquipmentId",
            new { from = from.Date, to = to.Date }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<PmSchedule>> GetPmSchedulesAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<PmSchedule>(new CommandDefinition(
            "SELECT * FROM PmSchedules WHERE IsActive = 1 ORDER BY NextPmDate",
            cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<PmRecord>> GetPmRecordsAsync(int? equipmentId, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var sql = equipmentId.HasValue
            ? "SELECT TOP 50 * FROM PmRecords WHERE EquipmentId = @equipmentId ORDER BY PmDate DESC"
            : "SELECT TOP 50 * FROM PmRecords ORDER BY PmDate DESC";
        var rows = await conn.QueryAsync<PmRecord>(new CommandDefinition(sql, new { equipmentId }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<int> CreatePmRecordAsync(PmRecord record, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition("""
            INSERT INTO PmRecords (PmScheduleId,EquipmentId,EquipmentName,PmDate,Technician,Result,FindingsNote,WorkTimeMinutes,CreatedAt)
            VALUES (@PmScheduleId,@EquipmentId,@EquipmentName,@PmDate,@Technician,@Result,@FindingsNote,@WorkTimeMinutes,@CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, record, cancellationToken: ct));
    }

    public async Task UpdatePmScheduleNextDateAsync(int scheduleId, DateTime nextDate, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE PmSchedules SET LastPmDate = CAST(GETDATE() AS DATE), NextPmDate = @nextDate WHERE Id = @scheduleId",
            new { scheduleId, nextDate = nextDate.Date }, cancellationToken: ct));
    }
}

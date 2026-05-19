using MSMES.Domain.Equipment;
using MSMES.Domain.Inventory;
using MSMES.Domain.Process;
using MSMES.Domain.Quality;

namespace MSMES.Application.Dashboard;

public sealed record DashboardKpiDto(
    decimal TodayProduced,
    decimal TodayDefect,
    decimal TodayTarget,
    int OpenSalesOrderCount,
    decimal OpenSalesOrderAmount,
    int InventoryNormal,
    int InventoryLow,
    int InventoryOut,
    int EquipmentRunning,
    int EquipmentStopped,
    int EquipmentMaintenance,
    int EquipmentBreakdown,
    decimal Recent7DaysDefectRate,
    int WorkOrderPlanned,
    int WorkOrderInProgress,
    int WorkOrderCompleted);

public sealed record DashboardChartDto(
    IReadOnlyList<DateTime> Dates,
    IReadOnlyList<decimal> Produced,
    IReadOnlyList<decimal> Defect);

public sealed record RecentWorkOrderDto(
    string WorkOrderNo,
    string ItemCode,
    decimal Quantity,
    string Status,
    DateTime CreatedAt);

public sealed record LowStockItemDto(
    string ItemCode,
    string ItemName,
    string WarehouseCode,
    decimal CurrentStock,
    decimal SafetyStock,
    string Unit,
    InventoryStatus Status);

public sealed record DashboardDto(
    DashboardKpiDto Kpi,
    DashboardChartDto ProductionChart,
    IReadOnlyList<RecentWorkOrderDto> RecentWorkOrders,
    IReadOnlyList<LowStockItemDto> LowStockAlerts);

public sealed record DashboardQuery(decimal TodayTarget = 1000m);

public sealed class DashboardHandler
{
    private readonly IInventoryRepository _inventory;
    private readonly IEquipmentRepository _equipment;
    private readonly IQualityRepository _quality;
    private readonly IProcessRepository _process;
    private readonly IDashboardRepository _dashboard;

    public DashboardHandler(
        IInventoryRepository inventory,
        IEquipmentRepository equipment,
        IQualityRepository quality,
        IProcessRepository process,
        IDashboardRepository dashboard)
    {
        _inventory = inventory;
        _equipment = equipment;
        _quality = quality;
        _process = process;
        _dashboard = dashboard;
    }

    public async Task<DashboardDto> HandleAsync(DashboardQuery q, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);

        var todayTotals = await _process.GetDailyTotalsAsync(today, ct);
        var invSummary = await _inventory.GetStatusSummaryAsync(ct);
        var eqSummary = await _equipment.GetStatusSummaryAsync(ct);
        var defectStats = await _quality.GetDefectStatsAsync(weekAgo, today.AddDays(1), ct);
        var backlog = await _dashboard.GetOpenSalesOrderBacklogAsync(ct);
        var woSummary = await _dashboard.GetWorkOrderStatusSummaryAsync(ct);

        var kpi = new DashboardKpiDto(
            todayTotals.Produced, todayTotals.Defect, q.TodayTarget,
            backlog.Count, backlog.Amount,
            invSummary.Normal, invSummary.Low, invSummary.Out,
            eqSummary.Running, eqSummary.Stopped, eqSummary.Maintenance, eqSummary.Breakdown,
            defectStats.DefectRate,
            woSummary.Planned, woSummary.InProgress, woSummary.Completed);

        var chartRows = await _dashboard.GetDailyProductionAsync(weekAgo, today, ct);
        var chart = new DashboardChartDto(
            chartRows.Select(r => r.Date).ToList(),
            chartRows.Select(r => r.Produced).ToList(),
            chartRows.Select(r => r.Defect).ToList());

        var recent = (await _dashboard.GetRecentWorkOrdersAsync(10, ct))
            .Select(r => new RecentWorkOrderDto(r.WorkOrderNo, r.ItemCode, r.Quantity, r.Status, r.CreatedAt))
            .ToList();

        var lowStock = (await _inventory.ListByStatusAsync(InventoryStatus.LowStock, ct))
            .Concat(await _inventory.ListByStatusAsync(InventoryStatus.OutOfStock, ct))
            .Select(i => new LowStockItemDto(i.ItemCode, i.ItemName, i.WarehouseCode, i.CurrentStock, i.SafetyStock, i.Unit, i.Status))
            .ToList();

        return new DashboardDto(kpi, chart, recent, lowStock);
    }
}

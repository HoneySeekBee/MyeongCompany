namespace MSMES.Application.Dashboard;

public interface IDashboardRepository
{
    // 수주 잔고: 미완료(=Closed/Cancelled 제외) 건수와 합계 금액
    Task<(int Count, decimal Amount)> GetOpenSalesOrderBacklogAsync(CancellationToken ct = default);

    // 작업지시 현황: 계획/진행/완료
    Task<(int Planned, int InProgress, int Completed)> GetWorkOrderStatusSummaryAsync(CancellationToken ct = default);

    // 일자별 생산수량 (라인차트용)
    Task<IReadOnlyList<(DateTime Date, decimal Produced, decimal Defect)>> GetDailyProductionAsync(DateTime from, DateTime to, CancellationToken ct = default);

    // 최근 작업지시 N건
    Task<IReadOnlyList<(string WorkOrderNo, string ItemCode, decimal Quantity, string Status, DateTime CreatedAt)>> GetRecentWorkOrdersAsync(int take, CancellationToken ct = default);
}

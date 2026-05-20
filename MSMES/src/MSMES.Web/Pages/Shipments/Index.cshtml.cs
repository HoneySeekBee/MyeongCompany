using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Shipment;

namespace MSMES.Web.Pages.Shipments;

public class IndexModel : PageModel
{
    private readonly IShipmentRepository _repo;

    public IndexModel(IShipmentRepository repo) => _repo = repo;

    public IReadOnlyList<Shipment> Shipments { get; private set; } = [];
    public int TotalCount { get; private set; }
    public int ShippedCount { get; private set; }
    public int DeliveredCount { get; private set; }
    public long MonthlyAmount { get; private set; }
    public IReadOnlyList<ShipmentItem> ShipmentItems { get; private set; } = [];
    public List<(string Date, int Count)> DailyShipmentTrend { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Shipments = await _repo.ListAllAsync(ct);

        TotalCount = Shipments.Count;
        ShippedCount = Shipments.Count(s => s.Status == ShipmentStatus.Shipped);
        DeliveredCount = Shipments.Count(s => s.Status == ShipmentStatus.Delivered);

        // 이번 달 출하 건수 기반 금액 추정 (출하 완료/배송완료 건 * 단위금액)
        var thisMonth = DateTime.Today;
        var monthlyShipments = Shipments
            .Where(s => s.ShipmentDate.Year == thisMonth.Year && s.ShipmentDate.Month == thisMonth.Month)
            .ToList();
        MonthlyAmount = monthlyShipments.Count; // 실제 금액 데이터 없으므로 건수 표시

        // 최근 7일 일별 출하 건수 트렌드
        DailyShipmentTrend = [];
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.Today.AddDays(-i);
            var count = Shipments.Count(s => s.ShipmentDate.Date == date.Date);
            DailyShipmentTrend.Add((date.ToString("MM/dd"), count));
        }

        ShipmentItems = await _repo.GetRecentItemsAsync(50, ct);
    }
}

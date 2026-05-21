using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.PurchaseOrder;

namespace MSMES.Web.Pages.PurchaseOrders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IPurchaseOrderRepository _repo;

    public IndexModel(IPurchaseOrderRepository repo) => _repo = repo;

    public IReadOnlyList<PurchaseOrder> Orders { get; private set; } = Array.Empty<PurchaseOrder>();
    public IReadOnlyList<PurchaseOrderItem> Items { get; private set; } = Array.Empty<PurchaseOrderItem>();

    public int TotalCount { get; private set; }
    public int PendingCount { get; private set; }
    public int ReceivedCount { get; private set; }
    public decimal MonthlyAmount { get; private set; }

    public IReadOnlyList<PurchaseOrder> DueSoonOrders { get; private set; } = Array.Empty<PurchaseOrder>();

    public int StatusDraft { get; private set; }
    public int StatusIssued { get; private set; }
    public int StatusReceived { get; private set; }

    // 필터 파라미터
    [BindProperty(SupportsGet = true)] public string? PoNo { get; set; }
    [BindProperty(SupportsGet = true)] public string? Supplier { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var allOrders = await _repo.ListAllAsync(ct);
        Items = await _repo.ListAllItemsAsync(ct);

        var today = DateTime.Today;
        var firstOfMonth = new DateTime(today.Year, today.Month, 1);

        // KPI 및 차트는 전체 데이터 기반
        TotalCount    = allOrders.Count;
        PendingCount  = allOrders.Count(o =>
            o.Status == PurchaseOrderStatus.Draft || o.Status == PurchaseOrderStatus.Issued);
        ReceivedCount = allOrders.Count(o => o.Status == PurchaseOrderStatus.Received);
        MonthlyAmount = allOrders
            .Where(o => o.OrderDate >= firstOfMonth && o.OrderDate < firstOfMonth.AddMonths(1))
            .Sum(o => o.TotalAmount);

        DueSoonOrders = allOrders
            .Where(o => o.Status != PurchaseOrderStatus.Received
                     && o.Status != PurchaseOrderStatus.Closed
                     && o.DueDate >= today
                     && o.DueDate <= today.AddDays(7))
            .OrderBy(o => o.DueDate)
            .ToList();

        StatusDraft    = allOrders.Count(o => o.Status == PurchaseOrderStatus.Draft);
        StatusIssued   = allOrders.Count(o => o.Status == PurchaseOrderStatus.Issued);
        StatusReceived = allOrders.Count(o => o.Status == PurchaseOrderStatus.Received);

        // 메모리 필터 적용 (쿼리 파라미터가 있을 경우)
        IEnumerable<PurchaseOrder> filtered = allOrders;

        if (!string.IsNullOrWhiteSpace(PoNo))
            filtered = filtered.Where(o => o.PurchaseOrderNo
                .Contains(PoNo, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(Supplier))
            filtered = filtered.Where(o =>
                o.SupplierName.Contains(Supplier, StringComparison.OrdinalIgnoreCase) ||
                o.SupplierCode.Contains(Supplier, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(Status) &&
            Enum.TryParse<PurchaseOrderStatus>(Status, ignoreCase: true, out var statusEnum))
            filtered = filtered.Where(o => o.Status == statusEnum);

        Orders = filtered.ToList();
    }
}

using Microsoft.AspNetCore.Authorization;
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

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Orders = await _repo.ListAllAsync(ct);
        Items = await _repo.ListAllItemsAsync(ct);

        var today = DateTime.Today;
        var firstOfMonth = new DateTime(today.Year, today.Month, 1);

        TotalCount = Orders.Count;
        PendingCount = Orders.Count(o =>
            o.Status == PurchaseOrderStatus.Draft || o.Status == PurchaseOrderStatus.Issued);
        ReceivedCount = Orders.Count(o => o.Status == PurchaseOrderStatus.Received);
        MonthlyAmount = Orders
            .Where(o => o.OrderDate >= firstOfMonth && o.OrderDate < firstOfMonth.AddMonths(1))
            .Sum(o => o.TotalAmount);

        DueSoonOrders = Orders
            .Where(o => o.Status != PurchaseOrderStatus.Received
                     && o.Status != PurchaseOrderStatus.Closed
                     && o.DueDate >= today
                     && o.DueDate <= today.AddDays(7))
            .OrderBy(o => o.DueDate)
            .ToList();

        StatusDraft    = Orders.Count(o => o.Status == PurchaseOrderStatus.Draft);
        StatusIssued   = Orders.Count(o => o.Status == PurchaseOrderStatus.Issued);
        StatusReceived = Orders.Count(o => o.Status == PurchaseOrderStatus.Received);
    }
}

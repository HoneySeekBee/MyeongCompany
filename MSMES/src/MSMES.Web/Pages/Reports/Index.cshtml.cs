using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Process;
using MSMES.Domain.Quality;
using MSMES.Domain.Shipment;

namespace MSMES.Web.Pages.Reports;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProcessRepository  _process;
    private readonly IQualityRepository  _quality;
    private readonly IShipmentRepository _shipment;

    public IndexModel(IProcessRepository process, IQualityRepository quality, IShipmentRepository shipment)
    {
        _process  = process;
        _quality  = quality;
        _shipment = shipment;
    }

    // Query params
    [BindProperty(SupportsGet = true)] public string    Period   { get; set; } = "daily";
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo   { get; set; }

    // Production
    public decimal TotalProduced { get; private set; }
    public decimal TotalDefect   { get; private set; }
    public decimal DefectRate    => TotalProduced == 0 ? 0 : Math.Round(TotalDefect / TotalProduced * 100, 1);
    public List<(string Label, decimal Produced, decimal Defect)> ProductionTrend { get; private set; } = [];

    // Quality  — QualityStatus enum: Pending=0, Passed=1, Failed=2, ConditionalPass=3
    public int     QualityInspected { get; private set; }
    public int     QualityPassed    { get; private set; }
    public int     QualityFailed    { get; private set; }
    public decimal PassRate         => QualityInspected == 0 ? 0 : Math.Round((decimal)QualityPassed / QualityInspected * 100, 1);

    // Shipment
    public int ShipmentTotal     { get; private set; }
    public int ShipmentDelivered { get; private set; }

    // Report metadata
    public DateTime ReportFrom { get; private set; }
    public DateTime ReportTo   { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        (ReportFrom, ReportTo) = Period switch
        {
            "weekly"  => (DateTime.Today.AddDays(-6), DateTime.Today),
            "monthly" => (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today),
            _ => DateFrom.HasValue && DateTo.HasValue
                ? (DateFrom.Value.Date, DateTo.Value.Date)
                : (DateTime.Today, DateTime.Today)
        };

        var from = ReportFrom;
        var to   = ReportTo.AddDays(1);   // exclusive upper bound

        // ── Production ──────────────────────────────────────────────────────
        var results = await _process.ListResultsAsync(from, to, ct);
        TotalProduced = results.Sum(r => r.ProducedQuantity);
        TotalDefect   = results.Sum(r => r.DefectQuantity);

        // Build daily trend
        for (var d = from.Date; d < to.Date; d = d.AddDays(1))
        {
            var dayResults = results.Where(r => r.StartTime.Date == d).ToList();
            ProductionTrend.Add((
                d.ToString("MM/dd"),
                dayResults.Sum(r => r.ProducedQuantity),
                dayResults.Sum(r => r.DefectQuantity)
            ));
        }

        // ── Quality ─────────────────────────────────────────────────────────
        // Use ListByDateAsync which filters by date range server-side
        var inspections = await _quality.ListByDateAsync(from, to, ct);
        QualityInspected = inspections.Count;
        QualityPassed    = inspections.Count(q => q.Result == QualityStatus.Passed);
        QualityFailed    = inspections.Count(q => q.Result == QualityStatus.Failed);

        // ── Shipment ─────────────────────────────────────────────────────────
        var shipments = await _shipment.ListAllAsync(ct);
        var shipped   = shipments.Where(s => s.ShipmentDate >= from && s.ShipmentDate < to).ToList();
        ShipmentTotal     = shipped.Count;
        ShipmentDelivered = shipped.Count(s => s.Status == ShipmentStatus.Delivered);
    }
}

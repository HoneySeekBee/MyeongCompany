using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Settings;

namespace MSMES.Web.Pages.Admin.Settings;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ISettingsRepository _settings;

    public IndexModel(ISettingsRepository settings) => _settings = settings;

    // ── 회사 정보 ──────────────────────────────────────────
    [BindProperty] public string CompanyName       { get; set; } = string.Empty;
    [BindProperty] public string CompanyBusinessNo { get; set; } = string.Empty;
    [BindProperty] public string CompanyCeo        { get; set; } = string.Empty;
    [BindProperty] public string CompanyPhone      { get; set; } = string.Empty;
    [BindProperty] public string CompanyAddress    { get; set; } = string.Empty;
    [BindProperty] public string CompanyEmail      { get; set; } = string.Empty;

    // ── 시스템 설정 ────────────────────────────────────────
    [BindProperty] public int    LowStockThreshold        { get; set; } = 10;
    [BindProperty] public bool   AlertEmailEnabled        { get; set; }
    [BindProperty] public int    SessionTimeoutMinutes    { get; set; } = 480;
    [BindProperty] public string WorkOrderPrefix          { get; set; } = "WO";
    [BindProperty] public string SalesOrderPrefix         { get; set; } = "SO";
    [BindProperty] public string PurchaseOrderPrefix      { get; set; } = "PO";

    public async Task OnGetAsync(CancellationToken ct)
    {
        var all = await _settings.GetAllAsync(ct);
        LoadFromDictionary(all);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var updatedBy = User.Identity?.Name ?? "System";

        var batch = new Dictionary<string, string>
        {
            ["Company.Name"]                     = CompanyName,
            ["Company.BusinessNo"]               = CompanyBusinessNo,
            ["Company.CEO"]                      = CompanyCeo,
            ["Company.Phone"]                    = CompanyPhone,
            ["Company.Address"]                  = CompanyAddress,
            ["Company.Email"]                    = CompanyEmail,
            ["System.LowStockThreshold"]         = LowStockThreshold.ToString(),
            ["System.AlertEmailEnabled"]         = AlertEmailEnabled.ToString().ToLowerInvariant(),
            ["System.SessionTimeoutMinutes"]     = SessionTimeoutMinutes.ToString(),
            ["System.DefaultWorkOrderPrefix"]    = WorkOrderPrefix,
            ["System.DefaultSalesOrderPrefix"]   = SalesOrderPrefix,
            ["System.DefaultPurchaseOrderPrefix"]= PurchaseOrderPrefix,
        };

        await _settings.UpsertManyAsync(batch, updatedBy, ct);

        TempData["SuccessMessage"] = "시스템 설정이 저장되었습니다.";
        return RedirectToPage();
    }

    // ── 헬퍼 ──────────────────────────────────────────────
    private void LoadFromDictionary(Dictionary<string, string> d)
    {
        CompanyName       = d.GetValueOrDefault("Company.Name",       string.Empty);
        CompanyBusinessNo = d.GetValueOrDefault("Company.BusinessNo", string.Empty);
        CompanyCeo        = d.GetValueOrDefault("Company.CEO",        string.Empty);
        CompanyPhone      = d.GetValueOrDefault("Company.Phone",      string.Empty);
        CompanyAddress    = d.GetValueOrDefault("Company.Address",    string.Empty);
        CompanyEmail      = d.GetValueOrDefault("Company.Email",      string.Empty);

        LowStockThreshold = int.TryParse(
            d.GetValueOrDefault("System.LowStockThreshold", "10"), out var lst) ? lst : 10;

        AlertEmailEnabled = string.Equals(
            d.GetValueOrDefault("System.AlertEmailEnabled", "false"), "true",
            StringComparison.OrdinalIgnoreCase);

        SessionTimeoutMinutes = int.TryParse(
            d.GetValueOrDefault("System.SessionTimeoutMinutes", "480"), out var sto) ? sto : 480;

        WorkOrderPrefix   = d.GetValueOrDefault("System.DefaultWorkOrderPrefix",    "WO");
        SalesOrderPrefix  = d.GetValueOrDefault("System.DefaultSalesOrderPrefix",   "SO");
        PurchaseOrderPrefix = d.GetValueOrDefault("System.DefaultPurchaseOrderPrefix", "PO");
    }
}

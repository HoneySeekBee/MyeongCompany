using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Domain.Process;
using MSMES.Domain.SalesOrder;
using MSMES.Domain.PurchaseOrder;

namespace MSMES.Web.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly IProcessRepository _process;
    private readonly ISalesOrderRepository _sales;
    private readonly IPurchaseOrderRepository _purchase;

    public ExportController(IProcessRepository process, ISalesOrderRepository sales, IPurchaseOrderRepository purchase)
    { _process = process; _sales = sales; _purchase = purchase; }

    [HttpGet("production")]
    public async Task<IActionResult> ExportProduction(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var from = today.AddDays(-30);
        var results = await _process.ListResultsAsync(from, today.AddDays(1), ct);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("생산실적");
        ws.Cell(1, 1).Value = "실적번호"; ws.Cell(1, 2).Value = "작업지시번호";
        ws.Cell(1, 3).Value = "공정코드"; ws.Cell(1, 4).Value = "작업자";
        ws.Cell(1, 5).Value = "생산수량"; ws.Cell(1, 6).Value = "불량수량";
        ws.Cell(1, 7).Value = "시작시간";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
        headerRow.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var r in results)
        {
            ws.Cell(row, 1).Value = r.ResultNo;
            ws.Cell(row, 2).Value = r.WorkOrderNo;
            ws.Cell(row, 3).Value = r.ProcessCode;
            ws.Cell(row, 4).Value = r.Operator;
            ws.Cell(row, 5).Value = (double)r.ProducedQuantity;
            ws.Cell(row, 6).Value = (double)r.DefectQuantity;
            ws.Cell(row, 7).Value = r.StartTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        var fileName = $"생산실적_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("sales-orders")]
    public async Task<IActionResult> ExportSalesOrders(CancellationToken ct)
    {
        var orders = await _sales.ListAsync(0, 500, ct);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("수주현황");
        ws.Cell(1, 1).Value = "수주번호"; ws.Cell(1, 2).Value = "고객명";
        ws.Cell(1, 3).Value = "상태"; ws.Cell(1, 4).Value = "수주일";
        ws.Cell(1, 5).Value = "납기일"; ws.Cell(1, 6).Value = "등록일";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
        headerRow.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var o in orders)
        {
            ws.Cell(row, 1).Value = o.SalesOrderNo;
            ws.Cell(row, 2).Value = o.CustomerName;
            ws.Cell(row, 3).Value = o.Status.ToString();
            ws.Cell(row, 4).Value = o.OrderDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 5).Value = o.DueDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 6).Value = o.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"수주현황_{DateTime.Today:yyyyMMdd}.xlsx");
    }

    [HttpGet("purchase-orders")]
    public async Task<IActionResult> ExportPurchaseOrders(CancellationToken ct)
    {
        var orders = await _purchase.ListAsync(0, 500, ct);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("발주현황");
        ws.Cell(1, 1).Value = "발주번호"; ws.Cell(1, 2).Value = "공급업체";
        ws.Cell(1, 3).Value = "상태"; ws.Cell(1, 4).Value = "발주일";
        ws.Cell(1, 5).Value = "납기일"; ws.Cell(1, 6).Value = "등록일";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
        headerRow.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var o in orders)
        {
            ws.Cell(row, 1).Value = o.PurchaseOrderNo;
            ws.Cell(row, 2).Value = o.SupplierName;
            ws.Cell(row, 3).Value = o.Status.ToString();
            ws.Cell(row, 4).Value = o.OrderDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 5).Value = o.DueDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 6).Value = o.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"발주현황_{DateTime.Today:yyyyMMdd}.xlsx");
    }
}

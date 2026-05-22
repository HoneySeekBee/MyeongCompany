using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MSMES.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public string? RequestId { get; private set; }
    public string? ExceptionMessage { get; private set; }
    public string? ExceptionStackTrace { get; private set; }
    public bool ShowDetails { get; private set; }

    public ErrorModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void OnGet()
    {
        RequestId = HttpContext.TraceIdentifier;
        ShowDetails = _env.IsDevelopment();

        if (ShowDetails)
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature?.Error is { } ex)
            {
                ExceptionMessage = ex.Message;
                ExceptionStackTrace = ex.ToString();
            }
        }
    }
}

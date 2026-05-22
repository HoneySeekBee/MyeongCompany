using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MSMES.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class StatusCodeModel : PageModel
{
    public int StatusCode { get; private set; }
    public string PageTitle { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string IconName { get; private set; } = string.Empty;
    public string IconClass { get; private set; } = string.Empty;

    public void OnGet([FromQuery(Name = "code")] int code = 0)
    {
        StatusCode = code;

        switch (code)
        {
            case 404:
                PageTitle  = "페이지를 찾을 수 없습니다";
                Description = "요청하신 페이지가 존재하지 않거나 이동되었습니다.";
                IconName   = "bi-search";
                IconClass  = "icon-404";
                break;

            case 403:
                PageTitle  = "접근 권한이 없습니다";
                Description = "이 페이지에 접근할 수 있는 권한이 없습니다.";
                IconName   = "bi-shield-lock";
                IconClass  = "icon-403";
                break;

            default:
                PageTitle  = "오류가 발생했습니다";
                Description = "요청을 처리하는 중 문제가 발생했습니다. 잠시 후 다시 시도해 주세요.";
                IconName   = "bi-exclamation-triangle";
                IconClass  = "icon-default";
                break;
        }
    }
}

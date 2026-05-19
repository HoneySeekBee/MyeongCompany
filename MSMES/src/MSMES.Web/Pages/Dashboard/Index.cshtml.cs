using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MSMES.Web.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    // 대시보드 페이지는 클라이언트 사이드에서 /api/dashboard 를 AJAX 호출하므로
    // PageModel에는 별도 로직이 필요하지 않습니다.
    public void OnGet() { }
}

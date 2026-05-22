using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MSMES.Web.Pages.Alerts;

[Authorize]
public class IndexModel : PageModel
{
    // 알림 데이터는 클라이언트에서 /api/alerts/live/* AJAX로 로드합니다.
    public void OnGet() { }
}

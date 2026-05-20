using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MSMES.Web.Pages.WorkOrders;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet() { }
}

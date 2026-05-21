using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Application.Common;
using MSMES.Domain.Common;

namespace MSMES.Web.Pages.Account;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public ChangePasswordModel(IUserRepository users, IPasswordHasher hasher)
    { _users = users; _hasher = hasher; }

    [BindProperty] public string CurrentPassword { get; set; } = string.Empty;
    [BindProperty] public string NewPassword     { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError(nameof(ConfirmPassword), "새 비밀번호가 일치하지 않습니다.");
            return Page();
        }
        if (NewPassword.Length < 6)
        {
            ModelState.AddModelError(nameof(NewPassword), "비밀번호는 6자 이상이어야 합니다.");
            return Page();
        }

        var userId = User.Identity?.Name ?? string.Empty;
        var user   = await _users.GetByUserIdAsync(userId, ct);
        if (user is null) return NotFound();

        if (!_hasher.Verify(CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(CurrentPassword), "현재 비밀번호가 올바르지 않습니다.");
            return Page();
        }

        user.PasswordHash = _hasher.Hash(NewPassword);
        await _users.UpdateAsync(user, ct);
        TempData["SuccessMessage"] = "비밀번호가 변경되었습니다.";
        return RedirectToPage();
    }
}

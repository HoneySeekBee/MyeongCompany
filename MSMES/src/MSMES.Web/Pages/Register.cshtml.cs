using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Application.Common;
using MSMES.Domain.Common;

namespace MSMES.Web.Pages;

public class RegisterModel : PageModel
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public RegisterModel(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    [BindProperty] public string UserId   { get; set; } = string.Empty;
    [BindProperty] public string Name     { get; set; } = string.Empty;
    [BindProperty] public string Email    { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string Confirm  { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError(string.Empty, "필수 항목을 모두 입력하세요.");
            return Page();
        }

        if (Password.Length < 6)
        {
            ModelState.AddModelError(string.Empty, "비밀번호는 6자 이상이어야 합니다.");
            return Page();
        }

        if (Password != Confirm)
        {
            ModelState.AddModelError(string.Empty, "비밀번호가 일치하지 않습니다.");
            return Page();
        }

        var existing = await _users.GetByUserIdAsync(UserId.Trim(), ct);
        if (existing is not null)
        {
            ModelState.AddModelError(string.Empty, "이미 사용 중인 아이디입니다.");
            return Page();
        }

        var user = new User
        {
            UserId       = UserId.Trim(),
            Name         = Name.Trim(),
            Email        = Email.Trim(),
            Role         = "Worker",
            PasswordHash = _hasher.Hash(Password),
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = "self-register"
        };

        await _users.CreateAsync(user, ct);

        TempData["RegisterSuccess"] = $"{Name}님, 가입이 완료되었습니다. 로그인하세요.";
        return RedirectToPage("/Login");
    }
}

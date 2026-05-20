using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Domain.Common;

namespace MSMES.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IUserRepository _users;
    private readonly MSMES.Application.Common.IPasswordHasher _hasher;

    public LoginModel(IUserRepository users, MSMES.Application.Common.IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    [BindProperty]
    public string UserId { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        // 이미 로그인되어 있으면 대시보드로
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError(string.Empty, "아이디와 비밀번호를 입력하세요.");
            return Page();
        }

        // 1. 유저 조회
        var user = await _users.GetByUserIdAsync(UserId.Trim(), ct);

        // 2. 비밀번호 검증
        if (user is null || !user.IsActive || !_hasher.Verify(Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "아이디 또는 비밀번호가 올바르지 않습니다.");
            return Page();
        }

        // 3. ClaimsPrincipal 생성
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("UserName", user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        // 4. 쿠키 로그인
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);

        // 5. 대시보드로 리다이렉트
        return RedirectToPage("/Index");
    }
}

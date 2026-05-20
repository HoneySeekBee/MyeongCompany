using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MSMES.Application.Common;
using MSMES.Domain.Common;

namespace MSMES.Web.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;

    public IndexModel(IUserRepository userRepo, IPasswordHasher hasher)
    {
        _userRepo = userRepo;
        _hasher   = hasher;
    }

    public IReadOnlyList<User> Users { get; private set; } = Array.Empty<User>();
    public int TotalUsers  { get; private set; }
    public int ActiveUsers { get; private set; }
    public string TopRole  { get; private set; } = string.Empty;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Users = await _userRepo.ListAllAsync(ct);
        TotalUsers  = Users.Count;
        ActiveUsers = Users.Count(u => u.IsActive);
        TopRole = Users
            .GroupBy(u => u.Role)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "-";
    }

    // ── 사용자 등록 ──────────────────────────────────────
    public sealed record CreateUserInput(
        string UserId,
        string Name,
        string Email,
        string Password,
        string Role
    );

    [BindProperty] public CreateUserInput? CreateInput { get; set; }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (CreateInput is null) return BadRequest();

        if (!ModelState.IsValid)
        {
            await OnGetAsync(ct);
            return Page();
        }

        var existing = await _userRepo.GetByUserIdAsync(CreateInput.UserId, ct);
        if (existing is not null)
        {
            ModelState.AddModelError(string.Empty, "이미 존재하는 사용자 ID입니다.");
            await OnGetAsync(ct);
            return Page();
        }

        var user = new User
        {
            UserId       = CreateInput.UserId,
            Name         = CreateInput.Name,
            Email        = CreateInput.Email,
            Role         = CreateInput.Role,
            PasswordHash = _hasher.Hash(CreateInput.Password),
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = User.Identity?.Name
        };

        await _userRepo.CreateAsync(user, ct);
        TempData["SuccessMessage"] = $"사용자 '{user.Name}'이(가) 등록되었습니다.";
        return RedirectToPage();
    }

    // ── 사용자 수정 ──────────────────────────────────────
    public sealed record UpdateUserInput(
        string UserId,
        string Name,
        string Email,
        string Role,
        bool ChangePassword,
        string? NewPassword
    );

    [BindProperty] public UpdateUserInput? UpdateInput { get; set; }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken ct)
    {
        if (UpdateInput is null) return BadRequest();

        var user = await _userRepo.GetByUserIdAsync(UpdateInput.UserId, ct);
        if (user is null) return NotFound();

        user.Name  = UpdateInput.Name;
        user.Email = UpdateInput.Email;
        user.Role  = UpdateInput.Role;

        if (UpdateInput.ChangePassword && !string.IsNullOrWhiteSpace(UpdateInput.NewPassword))
            user.PasswordHash = _hasher.Hash(UpdateInput.NewPassword);

        await _userRepo.UpdateAsync(user, ct);
        TempData["SuccessMessage"] = $"사용자 '{user.Name}'이(가) 수정되었습니다.";
        return RedirectToPage();
    }

    // ── 비활성화 ──────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteAsync(string userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByUserIdAsync(userId, ct);
        if (user is null) return NotFound();

        user.IsActive = false;
        await _userRepo.UpdateAsync(user, ct);
        TempData["SuccessMessage"] = $"사용자 '{user.Name}'이(가) 비활성화되었습니다.";
        return RedirectToPage();
    }
}

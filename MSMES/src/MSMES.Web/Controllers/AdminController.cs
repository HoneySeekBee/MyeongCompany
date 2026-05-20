using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Common;
using MSMES.Domain.Common;

namespace MSMES.Web.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;

    public AdminController(IUserRepository userRepo, IPasswordHasher hasher)
    {
        _userRepo = userRepo;
        _hasher   = hasher;
    }

    // GET /api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await _userRepo.ListAllAsync(ct);
        var result = users.Select(u => new
        {
            u.UserId,
            u.Name,
            u.Email,
            u.Role,
            u.IsActive,
            CreatedAt = u.CreatedAt
        });
        return Ok(result);
    }

    // POST /api/admin/users
    public sealed record CreateUserRequest(
        string UserId,
        string Name,
        string Email,
        string Password,
        string Role
    );

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) ||
            string.IsNullOrWhiteSpace(req.Name)   ||
            string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "필수 항목이 누락되었습니다." });

        if (req.Password.Length < 8)
            return BadRequest(new { message = "비밀번호는 8자 이상이어야 합니다." });

        var existing = await _userRepo.GetByUserIdAsync(req.UserId, ct);
        if (existing is not null)
            return Conflict(new { message = "이미 존재하는 사용자 ID입니다." });

        var user = new User
        {
            UserId       = req.UserId,
            Name         = req.Name,
            Email        = req.Email,
            Role         = req.Role,
            PasswordHash = _hasher.Hash(req.Password),
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = User.Identity?.Name
        };

        await _userRepo.CreateAsync(user, ct);
        return Created($"/api/admin/users/{user.UserId}", new { user.UserId, user.Name, user.Role });
    }

    // PUT /api/admin/users/{id}
    public sealed record UpdateUserRequest(
        string Name,
        string Email,
        string Role,
        bool ChangePassword,
        string? NewPassword
    );

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        var user = await _userRepo.GetByUserIdAsync(id, ct);
        if (user is null) return NotFound(new { message = "사용자를 찾을 수 없습니다." });

        user.Name  = req.Name;
        user.Email = req.Email;
        user.Role  = req.Role;

        if (req.ChangePassword && !string.IsNullOrWhiteSpace(req.NewPassword))
        {
            if (req.NewPassword.Length < 8)
                return BadRequest(new { message = "비밀번호는 8자 이상이어야 합니다." });

            user.PasswordHash = _hasher.Hash(req.NewPassword);
        }

        await _userRepo.UpdateAsync(user, ct);
        return Ok(new { user.UserId, user.Name, user.Role });
    }

    // DELETE /api/admin/users/{id} — 비활성화 (IsActive = false)
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeactivateUser(string id, CancellationToken ct)
    {
        var user = await _userRepo.GetByUserIdAsync(id, ct);
        if (user is null) return NotFound(new { message = "사용자를 찾을 수 없습니다." });

        user.IsActive = false;
        await _userRepo.UpdateAsync(user, ct);
        return Ok(new { message = $"사용자 '{user.Name}'이(가) 비활성화되었습니다." });
    }
}

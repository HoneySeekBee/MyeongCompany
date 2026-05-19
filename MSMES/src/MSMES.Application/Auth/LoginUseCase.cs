using MSMES.Application.Common;
using MSMES.Domain.Common;

namespace MSMES.Application.Auth;

public sealed record LoginCommand(string UserId, string Password);
public sealed record LoginResult(string Token, string UserId, string Name, string Role);

public sealed class LoginHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public LoginHandler(IUserRepository users, IPasswordHasher hasher, IJwtService jwt)
    {
        _users = users; _hasher = hasher; _jwt = jwt;
    }

    public async Task<LoginResult?> HandleAsync(LoginCommand cmd, CancellationToken ct = default)
    {
        var user = await _users.GetByUserIdAsync(cmd.UserId, ct);
        if (user is null || !user.IsActive) return null;
        if (!_hasher.Verify(cmd.Password, user.PasswordHash)) return null;
        var token = _jwt.GenerateToken(user);
        return new LoginResult(token, user.UserId, user.Name, user.Role);
    }
}

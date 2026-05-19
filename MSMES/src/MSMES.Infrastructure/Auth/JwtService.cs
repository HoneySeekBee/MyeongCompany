using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MSMES.Application.Common;
using MSMES.Domain.Common;

namespace MSMES.Infrastructure.Auth;

public sealed class JwtService : IJwtService
{
    private readonly JwtOptions _opt;
    public JwtService(IOptions<JwtOptions> opt) => _opt = opt.Value;

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
            new Claim(ClaimTypes.NameIdentifier, user.UserId),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opt.ExpiresMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token, out string? userId, out string? role)
    {
        userId = null; role = null;
        var handler = new JwtSecurityTokenHandler();
        var pars = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = _opt.Issuer,
            ValidateAudience = true, ValidAudience = _opt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey))
        };
        try
        {
            var principal = handler.ValidateToken(token, pars, out _);
            userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            role = principal.FindFirst(ClaimTypes.Role)?.Value;
            return true;
        }
        catch { return false; }
    }
}

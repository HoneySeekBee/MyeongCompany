using MSMES.Domain.Common;

namespace MSMES.Application.Common;

/// <summary>
/// JWT 토큰 발급/검증 인터페이스 (Application 레이어에 위치, Infrastructure가 구현)
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token, out string? userId, out string? role);
}

/// <summary>
/// 비밀번호 해싱/검증 인터페이스 (Application 레이어에 위치, Infrastructure가 구현)
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

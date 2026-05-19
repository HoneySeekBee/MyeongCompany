namespace MSMES.Infrastructure.Auth;

/// <summary>
/// Application.Common.IPasswordHasher 를 재-export 하는 alias
/// </summary>
public interface IPasswordHasher : MSMES.Application.Common.IPasswordHasher { }

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

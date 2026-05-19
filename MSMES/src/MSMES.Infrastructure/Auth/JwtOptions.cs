namespace MSMES.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "MSMES";
    public string Audience { get; set; } = "MSMES.Client";
    public string SigningKey { get; set; } = "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS";
    public int ExpiresMinutes { get; set; } = 480;
}

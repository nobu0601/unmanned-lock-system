namespace UnmannedLockSystem.Api.Configuration;

public class JwtSettings
{
    public string QrSecret { get; set; } = null!;
    public int QrTtlSeconds { get; set; } = 90;
    public string AdminSecret { get; set; } = null!;
    public int AdminTokenExpiryMinutes { get; set; } = 480;
}

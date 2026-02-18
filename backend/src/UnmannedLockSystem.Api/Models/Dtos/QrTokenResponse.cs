namespace UnmannedLockSystem.Api.Models.Dtos;

public class QrTokenResponse
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public int TtlSeconds { get; set; }
}

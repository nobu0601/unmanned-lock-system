namespace UnmannedLockSystem.Api.Models.Dtos;

public class AdminLoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AdminLoginResponse
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

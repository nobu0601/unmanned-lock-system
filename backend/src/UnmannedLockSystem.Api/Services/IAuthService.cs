using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Models.Entities;

namespace UnmannedLockSystem.Api.Services;

public class LineProfile
{
    public string UserId { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? PictureUrl { get; set; }
}

public interface IAuthService
{
    Task<LineProfile?> VerifyLineTokenAsync(string accessToken);
    Task<User> GetOrCreateLineUserAsync(LineProfile profile, Guid storeId);
    Task<AdminLoginResponse?> AdminLoginAsync(string email, string password);
}

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UnmannedLockSystem.Api.Configuration;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Models.Entities;
using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IOptions<JwtSettings> jwtSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthService> logger)
    {
        _db = db;
        _jwtSettings = jwtSettings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<LineProfile?> VerifyLineTokenAsync(string accessToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://api.line.me/v2/profile");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LINE token verification failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new LineProfile
            {
                UserId = root.GetProperty("userId").GetString()!,
                DisplayName = root.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
                PictureUrl = root.TryGetProperty("pictureUrl", out var pu) ? pu.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LINE token verification error");
            return null;
        }
    }

    public async Task<User> GetOrCreateLineUserAsync(LineProfile profile, Guid storeId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.LineUserId == profile.UserId);
        if (user != null)
        {
            user.DisplayName = profile.DisplayName ?? user.DisplayName;
            user.PictureUrl = profile.PictureUrl ?? user.PictureUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return user;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            LineUserId = profile.UserId,
            DisplayName = profile.DisplayName,
            PictureUrl = profile.PictureUrl,
            Role = UserRole.Customer,
            StoreId = storeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<AdminLoginResponse?> AdminLoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == UserRole.Admin);
        if (user == null || user.PasswordHash == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_jwtSettings.AdminTokenExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim("store_id", user.StoreId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.AdminSecret));
        var token = new JwtSecurityToken(
            issuer: "unmanned-lock-system",
            audience: "admin-dashboard",
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new AdminLoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UnmannedLockSystem.Api.Configuration;
using UnmannedLockSystem.Api.Models.Dtos;

namespace UnmannedLockSystem.Api.Services;

public class QrTokenService : IQrTokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;

    public QrTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.QrSecret));
    }

    public QrTokenResponse GenerateToken(Guid passId, Guid doorId, Guid storeId, Guid userId)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddSeconds(_settings.QrTtlSeconds);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, passId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("door_id", doorId.ToString()),
            new Claim("store_id", storeId.ToString()),
            new Claim("user_id", userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "unmanned-lock-system",
            audience: "device-scanner",
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new QrTokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires,
            TtlSeconds = _settings.QrTtlSeconds
        };
    }

    public QrTokenClaims? ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "unmanned-lock-system",
            ValidateAudience = true,
            ValidAudience = "device-scanner",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(5)
        };

        try
        {
            var principal = handler.ValidateToken(token, parameters, out _);

            return new QrTokenClaims
            {
                PassId = Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!),
                DoorId = Guid.Parse(principal.FindFirstValue("door_id")!),
                StoreId = Guid.Parse(principal.FindFirstValue("store_id")!),
                UserId = Guid.Parse(principal.FindFirstValue("user_id")!)
            };
        }
        catch
        {
            return null;
        }
    }
}

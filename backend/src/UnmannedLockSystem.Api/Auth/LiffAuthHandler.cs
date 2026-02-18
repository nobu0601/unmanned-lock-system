using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Services;

namespace UnmannedLockSystem.Api.Auth;

public class LiffAuthOptions : AuthenticationSchemeOptions { }

public class LiffAuthHandler : AuthenticationHandler<LiffAuthOptions>
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public LiffAuthHandler(
        IOptionsMonitor<LiffAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthService authService,
        AppDbContext db,
        IConfiguration config)
        : base(options, logger, encoder)
    {
        _authService = authService;
        _db = db;
        _config = config;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Mock mode: accept X-Mock-Line-User-Id header
        if (_config.GetValue<bool>("UseMockAuth"))
        {
            var mockUserId = Request.Headers["X-Mock-Line-User-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(mockUserId))
            {
                var defaultStore = await _db.Stores.FirstOrDefaultAsync();
                if (defaultStore == null)
                    return AuthenticateResult.Fail("No store configured");

                var user = await _authService.GetOrCreateLineUserAsync(
                    new LineProfile { UserId = mockUserId, DisplayName = $"MockUser_{mockUserId}" },
                    defaultStore.Id);

                return AuthenticateResult.Success(CreateTicket(user.Id, user.LineUserId, user.StoreId));
            }
        }

        // Real mode: validate LINE access token
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return AuthenticateResult.Fail("Missing or invalid Authorization header");

        var token = authHeader["Bearer ".Length..];
        var profile = await _authService.VerifyLineTokenAsync(token);
        if (profile == null)
            return AuthenticateResult.Fail("Invalid LINE access token");

        var store = await _db.Stores.FirstOrDefaultAsync();
        if (store == null)
            return AuthenticateResult.Fail("No store configured");

        var appUser = await _authService.GetOrCreateLineUserAsync(profile, store.Id);
        return AuthenticateResult.Success(CreateTicket(appUser.Id, appUser.LineUserId, appUser.StoreId));
    }

    private AuthenticationTicket CreateTicket(Guid userId, string lineUserId, Guid storeId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("line_user_id", lineUserId),
            new Claim("store_id", storeId.ToString())
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationTicket(principal, Scheme.Name);
    }
}

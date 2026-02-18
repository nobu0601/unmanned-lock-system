using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace UnmannedLockSystem.Api.Auth;

public class DeviceAuthOptions : AuthenticationSchemeOptions { }

public class DeviceAuthHandler : AuthenticationHandler<DeviceAuthOptions>
{
    private readonly IConfiguration _config;

    public DeviceAuthHandler(
        IOptionsMonitor<DeviceAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration config)
        : base(options, logger, encoder)
    {
        _config = config;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = Request.Headers["X-Device-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Device-Key header"));

        var expectedKey = _config["DeviceApiKey"];
        if (apiKey != expectedKey)
            return Task.FromResult(AuthenticateResult.Fail("Invalid device API key"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "device"),
            new Claim(ClaimTypes.Role, "device")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

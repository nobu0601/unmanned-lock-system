using Microsoft.AspNetCore.Mvc;
using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Services;

namespace UnmannedLockSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("line-login")]
    public async Task<IActionResult> LineLogin([FromBody] LineLoginRequest request)
    {
        var profile = await _authService.VerifyLineTokenAsync(request.AccessToken);
        if (profile == null)
            return Unauthorized(new { error = "Invalid LINE access token" });

        return Ok(new { userId = profile.UserId, displayName = profile.DisplayName });
    }

    [HttpPost("admin/login")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
    {
        var result = await _authService.AdminLoginAsync(request.Email, request.Password);
        if (result == null)
            return Unauthorized(new { error = "Invalid credentials" });

        return Ok(result);
    }
}

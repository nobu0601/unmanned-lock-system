using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnmannedLockSystem.Api.Services;

namespace UnmannedLockSystem.Api.Controllers;

[ApiController]
[Route("api/passes")]
[Authorize(Policy = "Customer")]
public class PassesController : ControllerBase
{
    private readonly IPassService _passService;
    private readonly IQrTokenService _qrTokenService;

    public PassesController(IPassService passService, IQrTokenService qrTokenService)
    {
        _passService = passService;
        _qrTokenService = qrTokenService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPasses()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var passes = await _passService.GetPassesByUserAsync(userId);
        return Ok(passes);
    }

    [HttpPost("{id}/qr")]
    public async Task<IActionResult> GenerateQr(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pass = await _passService.GetPassByIdAsync(id);

        if (pass == null)
            return NotFound(new { error = "Pass not found" });

        if (pass.UserId != userId)
            return Forbid();

        if (pass.Status != Models.Enums.PassStatus.Active)
            return BadRequest(new { error = $"Pass is {pass.Status}" });

        if (DateTime.UtcNow > pass.ValidTo)
            return BadRequest(new { error = "Pass has expired" });

        var qr = _qrTokenService.GenerateToken(pass.Id, pass.DoorId, pass.StoreId, userId);
        return Ok(qr);
    }
}

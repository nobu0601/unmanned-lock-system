using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Services;

namespace UnmannedLockSystem.Api.Controllers;

[ApiController]
[Route("api/device")]
[Authorize(Policy = "Device")]
public class DeviceController : ControllerBase
{
    private readonly IScanService _scanService;

    public DeviceController(IScanService scanService)
    {
        _scanService = scanService;
    }

    [HttpPost("scan")]
    [EnableRateLimiting("DeviceScan")]
    public async Task<IActionResult> Scan([FromBody] ScanRequest request)
    {
        var result = await _scanService.ProcessScanAsync(request.Token);
        return Ok(result);
    }
}

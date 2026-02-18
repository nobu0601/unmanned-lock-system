using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Services;

namespace UnmannedLockSystem.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IPassService _passService;
    private readonly AppDbContext _db;

    public AdminController(IPassService passService, AppDbContext db)
    {
        _passService = passService;
        _db = db;
    }

    [HttpGet("passes")]
    public async Task<IActionResult> GetPasses([FromQuery] Guid? storeId)
    {
        var passes = await _passService.GetAllPassesAsync(storeId);
        return Ok(passes);
    }

    [HttpPost("passes/{id}/revoke")]
    public async Task<IActionResult> RevokePass(Guid id)
    {
        var result = await _passService.RevokePassAsync(id);
        if (!result)
            return NotFound(new { error = "Pass not found" });

        return Ok(new { message = "Pass revoked" });
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _db.AccessLogs
            .Include(l => l.AccessPass)
                .ThenInclude(p => p.User)
            .AsQueryable();

        if (storeId.HasValue)
            query = query.Where(l => l.StoreId == storeId.Value);
        if (from.HasValue)
            query = query.Where(l => l.ScannedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.ScannedAt <= to.Value);

        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.ScannedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AccessLogDto
            {
                Id = l.Id,
                AccessPassId = l.AccessPassId,
                UserDisplayName = l.AccessPass.User.DisplayName,
                DoorName = l.DoorId.ToString(),
                Result = l.Result,
                DenialReason = l.DenialReason,
                ScannedAt = l.ScannedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = logs });
    }
}

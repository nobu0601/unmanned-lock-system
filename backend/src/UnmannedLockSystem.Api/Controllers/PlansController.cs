using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Dtos;

namespace UnmannedLockSystem.Api.Controllers;

[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlansController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlans([FromQuery] Guid? storeId)
    {
        var query = _db.Plans.Where(p => p.IsActive);
        if (storeId.HasValue)
            query = query.Where(p => p.StoreId == storeId.Value);

        var plans = await query
            .OrderBy(p => p.PriceYen)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                PlanType = p.PlanType,
                PriceYen = p.PriceYen,
                DurationMinutes = p.DurationMinutes
            })
            .ToListAsync();

        return Ok(plans);
    }
}

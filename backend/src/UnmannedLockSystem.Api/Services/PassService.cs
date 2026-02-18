using Microsoft.EntityFrameworkCore;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Models.Entities;
using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Services;

public class PassService : IPassService
{
    private readonly AppDbContext _db;

    public PassService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PassDto>> GetPassesByUserAsync(Guid userId)
    {
        return await _db.AccessPasses
            .Include(p => p.Order).ThenInclude(o => o.Plan)
            .Include(p => p.Store)
            .Include(p => p.Door)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<List<PassDto>> GetAllPassesAsync(Guid? storeId = null)
    {
        var query = _db.AccessPasses
            .Include(p => p.Order).ThenInclude(o => o.Plan)
            .Include(p => p.Store)
            .Include(p => p.Door)
            .AsQueryable();

        if (storeId.HasValue)
            query = query.Where(p => p.StoreId == storeId.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<AccessPass?> GetPassByIdAsync(Guid passId)
    {
        return await _db.AccessPasses
            .Include(p => p.Order).ThenInclude(o => o.Plan)
            .Include(p => p.Store)
            .Include(p => p.Door)
            .FirstOrDefaultAsync(p => p.Id == passId);
    }

    public async Task<AccessPass> CreatePassFromOrderAsync(Order order)
    {
        var plan = await _db.Plans.FindAsync(order.PlanId)
            ?? throw new InvalidOperationException($"Plan {order.PlanId} not found");

        var door = await _db.Doors.FirstOrDefaultAsync(d => d.StoreId == order.StoreId)
            ?? throw new InvalidOperationException($"No door found for store {order.StoreId}");

        var now = DateTime.UtcNow;
        var pass = new AccessPass
        {
            Id = Guid.NewGuid(),
            UserId = order.UserId,
            OrderId = order.Id,
            StoreId = order.StoreId,
            DoorId = door.Id,
            Status = PassStatus.Active,
            ValidFrom = now,
            ValidTo = now.AddMinutes(plan.DurationMinutes),
            MaxUses = plan.MaxUses,
            UsedCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.AccessPasses.Add(pass);
        await _db.SaveChangesAsync();
        return pass;
    }

    public async Task<bool> RevokePassAsync(Guid passId)
    {
        var pass = await _db.AccessPasses.FindAsync(passId);
        if (pass == null) return false;

        pass.Status = PassStatus.Revoked;
        pass.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static PassDto ToDto(AccessPass p)
    {
        return new PassDto
        {
            Id = p.Id,
            PlanName = p.Order.Plan.Name,
            Status = p.Status,
            ValidFrom = p.ValidFrom,
            ValidTo = p.ValidTo,
            MaxUses = p.MaxUses,
            UsedCount = p.UsedCount,
            StoreId = p.StoreId,
            StoreName = p.Store.Name,
            DoorId = p.DoorId,
            DoorName = p.Door.Name,
            CreatedAt = p.CreatedAt
        };
    }
}

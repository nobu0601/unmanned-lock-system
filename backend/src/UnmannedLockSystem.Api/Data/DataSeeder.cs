using Microsoft.EntityFrameworkCore;
using UnmannedLockSystem.Api.Models.Entities;
using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Data;

public static class DataSeeder
{
    // Well-known seed IDs
    public static readonly Guid DefaultStoreId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DefaultDoorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Stores.AnyAsync())
            return;

        // Default Store
        var store = new Store
        {
            Id = DefaultStoreId,
            Name = "Default Store",
            Address = "Tokyo, Japan",
            Timezone = "Asia/Tokyo",
            CreatedAt = DateTime.UtcNow
        };
        db.Stores.Add(store);

        // Default Door
        var door = new Door
        {
            Id = DefaultDoorId,
            StoreId = DefaultStoreId,
            Name = "Main Entrance",
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };
        db.Doors.Add(door);

        // Plans
        db.Plans.AddRange(
            new Plan
            {
                Id = Guid.NewGuid(),
                StoreId = DefaultStoreId,
                Name = "1時間パス",
                PlanType = PlanType.OneTime,
                PriceYen = 600,
                DurationMinutes = 60,
                MaxUses = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = Guid.NewGuid(),
                StoreId = DefaultStoreId,
                Name = "1日パス",
                PlanType = PlanType.OneTime,
                PriceYen = 2400,
                DurationMinutes = 1440,
                MaxUses = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = Guid.NewGuid(),
                StoreId = DefaultStoreId,
                Name = "月額プラン",
                PlanType = PlanType.Subscription,
                PriceYen = 9800,
                DurationMinutes = 43200,
                MaxUses = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Admin User
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            LineUserId = "admin",
            DisplayName = "Administrator",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin,
            StoreId = DefaultStoreId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Test Customer
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            LineUserId = "test-user-001",
            DisplayName = "Test Customer",
            Role = UserRole.Customer,
            StoreId = DefaultStoreId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using UnmannedLockSystem.Api.Models.Entities;
using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Door> Doors => Set<Door>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AccessPass> AccessPasses => Set<AccessPass>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store enum values as strings
        modelBuilder.Entity<User>()
            .Property(e => e.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Plan>()
            .Property(e => e.PlanType)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<AccessPass>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<AccessLog>()
            .Property(e => e.Result)
            .HasConversion<string>();

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.LineUserId).IsUnique();
            e.HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
            e.HasOne(u => u.Store).WithMany().HasForeignKey(u => u.StoreId);
        });

        // Door
        modelBuilder.Entity<Door>(e =>
        {
            e.HasOne(d => d.Store).WithMany(s => s.Doors).HasForeignKey(d => d.StoreId);
        });

        // Plan
        modelBuilder.Entity<Plan>(e =>
        {
            e.HasOne(p => p.Store).WithMany(s => s.Plans).HasForeignKey(p => p.StoreId);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.StripeSessionId).IsUnique().HasFilter("\"StripeSessionId\" IS NOT NULL");
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
            e.HasOne(o => o.Plan).WithMany().HasForeignKey(o => o.PlanId);
            e.HasOne(o => o.Store).WithMany().HasForeignKey(o => o.StoreId);
        });

        // AccessPass
        modelBuilder.Entity<AccessPass>(e =>
        {
            e.HasIndex(p => new { p.UserId, p.Status });
            e.HasIndex(p => new { p.StoreId, p.Status });
            e.HasOne(p => p.User).WithMany(u => u.AccessPasses).HasForeignKey(p => p.UserId);
            e.HasOne(p => p.Order).WithOne(o => o.AccessPass).HasForeignKey<AccessPass>(p => p.OrderId);
            e.HasOne(p => p.Store).WithMany().HasForeignKey(p => p.StoreId);
            e.HasOne(p => p.Door).WithMany().HasForeignKey(p => p.DoorId);
        });

        // AccessLog
        modelBuilder.Entity<AccessLog>(e =>
        {
            e.HasIndex(l => l.ScannedAt);
            e.HasIndex(l => new { l.StoreId, l.ScannedAt });
            e.HasOne(l => l.AccessPass).WithMany(p => p.AccessLogs).HasForeignKey(l => l.AccessPassId);
        });

        // WebhookEvent
        modelBuilder.Entity<WebhookEvent>(e =>
        {
            e.HasIndex(w => w.EventId).IsUnique();
        });
    }
}

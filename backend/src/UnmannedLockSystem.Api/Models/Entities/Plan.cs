using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Entities;

public class Plan
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = null!;
    public PlanType PlanType { get; set; } = PlanType.OneTime;
    public int PriceYen { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxUses { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Section 12 hook: Stripe subscription price ID
    public string? StripePriceId { get; set; }

    public Store Store { get; set; } = null!;
}

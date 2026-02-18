using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public Guid StoreId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public int AmountYen { get; set; }
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Section 12 hooks
    public string? CouponCode { get; set; }   // 12.7
    public Guid? BookingId { get; set; }       // 12.4

    public User User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public AccessPass? AccessPass { get; set; }
}

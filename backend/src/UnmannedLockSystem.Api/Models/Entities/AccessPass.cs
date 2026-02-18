using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Entities;

public class AccessPass
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public Guid StoreId { get; set; }
    public Guid DoorId { get; set; }
    public PassStatus Status { get; set; } = PassStatus.Active;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int MaxUses { get; set; } = 1;
    public int UsedCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Section 12 hooks
    public Guid? ZoneId { get; set; }     // 12.3
    public Guid? SeatId { get; set; }     // 12.3
    public Guid? BookingId { get; set; }  // 12.4

    public User User { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public Door Door { get; set; } = null!;
    public ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
}

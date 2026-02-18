using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Entities;

public class AccessLog
{
    public Guid Id { get; set; }
    public Guid AccessPassId { get; set; }
    public Guid StoreId { get; set; }
    public Guid DoorId { get; set; }
    public AccessResult Result { get; set; }
    public string? DenialReason { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    // Section 12.2 hook: device link
    public Guid? DeviceId { get; set; }

    public AccessPass AccessPass { get; set; } = null!;
}

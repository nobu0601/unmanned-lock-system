using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Dtos;

public class PassDto
{
    public Guid Id { get; set; }
    public string PlanName { get; set; } = null!;
    public PassStatus Status { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int MaxUses { get; set; }
    public int UsedCount { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = null!;
    public Guid DoorId { get; set; }
    public string DoorName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

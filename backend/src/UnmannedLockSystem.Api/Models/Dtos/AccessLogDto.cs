using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Dtos;

public class AccessLogDto
{
    public Guid Id { get; set; }
    public Guid AccessPassId { get; set; }
    public string? UserDisplayName { get; set; }
    public string DoorName { get; set; } = null!;
    public AccessResult Result { get; set; }
    public string? DenialReason { get; set; }
    public DateTime ScannedAt { get; set; }
}

namespace UnmannedLockSystem.Api.Models.Entities;

public class Door
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Section 12.3 hook: zone link
    public Guid? ZoneId { get; set; }

    public Store Store { get; set; } = null!;
}

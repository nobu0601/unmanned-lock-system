namespace UnmannedLockSystem.Api.Models.Entities;

public class Store
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string Timezone { get; set; } = "Asia/Tokyo";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Door> Doors { get; set; } = new List<Door>();
    public ICollection<Plan> Plans { get; set; } = new List<Plan>();
}

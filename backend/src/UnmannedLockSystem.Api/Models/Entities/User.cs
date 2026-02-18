using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string LineUserId { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? PictureUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public Guid StoreId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Section 12.6 hook: membership link
    public Guid? MembershipId { get; set; }

    public Store Store { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<AccessPass> AccessPasses { get; set; } = new List<AccessPass>();
}

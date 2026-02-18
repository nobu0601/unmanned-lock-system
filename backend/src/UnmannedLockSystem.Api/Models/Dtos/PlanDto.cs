using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Models.Dtos;

public class PlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public PlanType PlanType { get; set; }
    public int PriceYen { get; set; }
    public int DurationMinutes { get; set; }
}

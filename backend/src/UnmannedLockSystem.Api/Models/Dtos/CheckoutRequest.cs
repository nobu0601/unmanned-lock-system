namespace UnmannedLockSystem.Api.Models.Dtos;

public class CheckoutRequest
{
    public Guid PlanId { get; set; }
    public Guid StoreId { get; set; }
    public Guid DoorId { get; set; }
    public string SuccessUrl { get; set; } = null!;
    public string CancelUrl { get; set; } = null!;
}

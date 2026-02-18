namespace UnmannedLockSystem.Api.Models.Dtos;

public class CheckoutResponse
{
    public string CheckoutUrl { get; set; } = null!;
    public Guid OrderId { get; set; }
}

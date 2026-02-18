using UnmannedLockSystem.Api.Models.Entities;

namespace UnmannedLockSystem.Api.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid userId, Guid planId, Guid storeId);
    Task<Order?> GetByStripeSessionIdAsync(string sessionId);
    Task MarkAsPaidAsync(Guid orderId, string? paymentIntentId);
}

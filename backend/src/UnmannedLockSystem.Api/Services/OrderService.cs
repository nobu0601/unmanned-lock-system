using Microsoft.EntityFrameworkCore;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Entities;
using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order> CreateOrderAsync(Guid userId, Guid planId, Guid storeId)
    {
        var plan = await _db.Plans.FindAsync(planId)
            ?? throw new InvalidOperationException($"Plan {planId} not found");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            StoreId = storeId,
            Status = OrderStatus.Pending,
            AmountYen = plan.PriceYen,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetByStripeSessionIdAsync(string sessionId)
    {
        return await _db.Orders.FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);
    }

    public async Task MarkAsPaidAsync(Guid orderId, string? paymentIntentId)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found");

        order.Status = OrderStatus.Paid;
        order.StripePaymentIntentId = paymentIntentId;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}

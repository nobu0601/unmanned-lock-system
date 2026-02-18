using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnmannedLockSystem.Api.Adapters.Payment;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Services;

namespace UnmannedLockSystem.Api.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize(Policy = "Customer")]
public class CheckoutController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentProviderAdapter _payment;
    private readonly AppDbContext _db;

    public CheckoutController(IOrderService orderService, IPaymentProviderAdapter payment, AppDbContext db)
    {
        _orderService = orderService;
        _payment = payment;
        _db = db;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var plan = await _db.Plans.FindAsync(request.PlanId);
        if (plan == null)
            return BadRequest(new { error = "Plan not found" });

        var order = await _orderService.CreateOrderAsync(userId, request.PlanId, request.StoreId);

        var checkout = await _payment.CreateCheckoutSessionAsync(
            order.Id, plan.PriceYen, plan.Name,
            request.SuccessUrl, request.CancelUrl,
            new Dictionary<string, string> { ["order_id"] = order.Id.ToString() });

        order.StripeSessionId = checkout.SessionId;
        await _db.SaveChangesAsync();

        return Ok(new CheckoutResponse
        {
            CheckoutUrl = checkout.CheckoutUrl,
            OrderId = order.Id
        });
    }

    /// <summary>
    /// Mock: simulate payment completion (dev only)
    /// </summary>
    [HttpPost("mock-complete/{orderId}")]
    public async Task<IActionResult> MockComplete(Guid orderId, [FromServices] IPassService passService, [FromServices] IConfiguration config)
    {
        if (!config.GetValue<bool>("UseMockPayment"))
            return NotFound();

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        await _orderService.MarkAsPaidAsync(orderId, "mock_pi_" + Guid.NewGuid().ToString("N"));
        var pass = await passService.CreatePassFromOrderAsync(order);

        return Ok(new { orderId, passId = pass.Id, message = "Mock payment completed, pass created" });
    }
}

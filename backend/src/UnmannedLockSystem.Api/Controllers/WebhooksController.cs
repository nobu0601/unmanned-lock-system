using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnmannedLockSystem.Api.Adapters.Payment;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Entities;
using UnmannedLockSystem.Api.Services;

namespace UnmannedLockSystem.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentProviderAdapter _payment;
    private readonly IOrderService _orderService;
    private readonly IPassService _passService;
    private readonly AppDbContext _db;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentProviderAdapter payment,
        IOrderService orderService,
        IPassService passService,
        AppDbContext db,
        ILogger<WebhooksController> logger)
    {
        _payment = payment;
        _orderService = orderService;
        _passService = passService;
        _db = db;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";

        if (!_payment.ValidateWebhookSignature(payload, signature))
        {
            _logger.LogWarning("Webhook signature validation failed");
            return BadRequest("Invalid signature");
        }

        var parsed = _payment.ParseWebhookEvent(payload);

        // Idempotency check
        var existing = await _db.WebhookEvents
            .FirstOrDefaultAsync(e => e.EventId == parsed.EventId);
        if (existing?.Status == "processed")
        {
            _logger.LogInformation("Webhook {EventId} already processed, skipping", parsed.EventId);
            return Ok();
        }

        // Record webhook event
        var webhookEvent = existing ?? new WebhookEvent
        {
            Id = Guid.NewGuid(),
            Provider = "stripe",
            EventId = parsed.EventId,
            EventType = parsed.EventType,
            Status = "received",
            Payload = payload,
            ReceivedAt = DateTime.UtcNow
        };

        if (existing == null)
            _db.WebhookEvents.Add(webhookEvent);

        try
        {
            if (parsed.EventType == "checkout.session.completed" && parsed.SessionId != null)
            {
                var order = await _orderService.GetByStripeSessionIdAsync(parsed.SessionId);
                if (order != null)
                {
                    await _orderService.MarkAsPaidAsync(order.Id, parsed.PaymentIntentId);
                    await _passService.CreatePassFromOrderAsync(order);
                    _logger.LogInformation("Order {OrderId} paid, pass created", order.Id);
                }
            }

            webhookEvent.Status = "processed";
            webhookEvent.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook processing failed for {EventId}", parsed.EventId);
            webhookEvent.Status = "failed";
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}

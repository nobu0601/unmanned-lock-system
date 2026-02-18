using Microsoft.Extensions.Logging;

namespace UnmannedLockSystem.Api.Adapters.Payment;

public class MockPaymentAdapter : IPaymentProviderAdapter
{
    private readonly ILogger<MockPaymentAdapter> _logger;

    public MockPaymentAdapter(ILogger<MockPaymentAdapter> logger)
    {
        _logger = logger;
    }

    public Task<CreateCheckoutResult> CreateCheckoutSessionAsync(
        Guid orderId, int amountYen, string planName,
        string successUrl, string cancelUrl,
        Dictionary<string, string>? metadata = null)
    {
        var sessionId = $"mock_session_{Guid.NewGuid():N}";
        _logger.LogInformation("[MockPayment] Created checkout session {SessionId} for order {OrderId}, amount: {Amount}円",
            sessionId, orderId, amountYen);

        // Mock: redirect directly to success URL with session_id
        var checkoutUrl = $"{successUrl}?session_id={sessionId}&mock=true";

        return Task.FromResult(new CreateCheckoutResult
        {
            SessionId = sessionId,
            CheckoutUrl = checkoutUrl
        });
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        _logger.LogInformation("[MockPayment] Webhook signature validation skipped (mock mode)");
        return true;
    }

    public WebhookEventParsed ParseWebhookEvent(string payload)
    {
        _logger.LogInformation("[MockPayment] Parsing mock webhook event");
        return System.Text.Json.JsonSerializer.Deserialize<WebhookEventParsed>(payload)
            ?? throw new InvalidOperationException("Failed to parse mock webhook event");
    }
}

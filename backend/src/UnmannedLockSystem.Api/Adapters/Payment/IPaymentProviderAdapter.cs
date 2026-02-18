namespace UnmannedLockSystem.Api.Adapters.Payment;

public class CreateCheckoutResult
{
    public string SessionId { get; set; } = null!;
    public string CheckoutUrl { get; set; } = null!;
}

public class WebhookEventParsed
{
    public string EventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string? SessionId { get; set; }
    public string? PaymentIntentId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public interface IPaymentProviderAdapter
{
    Task<CreateCheckoutResult> CreateCheckoutSessionAsync(
        Guid orderId, int amountYen, string planName,
        string successUrl, string cancelUrl,
        Dictionary<string, string>? metadata = null);

    bool ValidateWebhookSignature(string payload, string signature);

    WebhookEventParsed ParseWebhookEvent(string payload);
}

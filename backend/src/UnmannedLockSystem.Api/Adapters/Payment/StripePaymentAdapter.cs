using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using UnmannedLockSystem.Api.Configuration;

namespace UnmannedLockSystem.Api.Adapters.Payment;

public class StripePaymentAdapter : IPaymentProviderAdapter
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentAdapter> _logger;

    public StripePaymentAdapter(IOptions<StripeSettings> settings, ILogger<StripePaymentAdapter> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<CreateCheckoutResult> CreateCheckoutSessionAsync(
        Guid orderId, int amountYen, string planName,
        string successUrl, string cancelUrl,
        Dictionary<string, string>? metadata = null)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "jpy",
                        UnitAmount = amountYen,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = planName
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = metadata ?? new Dictionary<string, string>()
        };
        options.Metadata["order_id"] = orderId.ToString();

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Created Stripe checkout session {SessionId} for order {OrderId}", session.Id, orderId);

        return new CreateCheckoutResult
        {
            SessionId = session.Id,
            CheckoutUrl = session.Url
        };
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            EventUtility.ConstructEvent(payload, signature, _settings.WebhookSecret);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Stripe webhook signature validation failed: {Message}", ex.Message);
            return false;
        }
    }

    public WebhookEventParsed ParseWebhookEvent(string payload)
    {
        var stripeEvent = EventUtility.ParseEvent(payload);
        var result = new WebhookEventParsed
        {
            EventId = stripeEvent.Id,
            EventType = stripeEvent.Type
        };

        if (stripeEvent.Data.Object is Session session)
        {
            result.SessionId = session.Id;
            result.PaymentIntentId = session.PaymentIntentId;
            result.Metadata = session.Metadata?.ToDictionary(k => k.Key, v => v.Value)
                ?? new Dictionary<string, string>();
        }

        return result;
    }
}

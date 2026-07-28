using Microsoft.Extensions.Options;

namespace Vennu.Api.Webhooks;

public sealed class StripeWebhookEventVerifier : IStripeWebhookEventVerifier
{
    private readonly StripeWebhookOptions options;
    private readonly TimeProvider timeProvider;

    public StripeWebhookEventVerifier(
        IOptions<StripeWebhookOptions> options,
        TimeProvider timeProvider)
    {
        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public Stripe.Event Verify(string payload, string signatureHeader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(signatureHeader);

        return Stripe.EventUtility.ConstructEvent(
            payload,
            signatureHeader,
            options.SigningSecret,
            options.ToleranceSeconds,
            timeProvider.GetUtcNow().ToUnixTimeSeconds());
    }
}

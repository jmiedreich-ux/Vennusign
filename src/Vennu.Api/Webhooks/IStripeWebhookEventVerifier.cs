namespace Vennu.Api.Webhooks;

public interface IStripeWebhookEventVerifier
{
    Stripe.Event Verify(string payload, string signatureHeader);
}

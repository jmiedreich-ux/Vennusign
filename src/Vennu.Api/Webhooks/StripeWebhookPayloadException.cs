namespace Vennu.Api.Webhooks;

public sealed class StripeWebhookPayloadException : Exception
{
    public StripeWebhookPayloadException(string message)
        : base(message)
    {
    }
}

namespace Vennu.Api.Webhooks;

public sealed class StripeWebhookOptions
{
    public const string SectionName = "Stripe:Webhook";

    public string SigningSecret { get; set; } = string.Empty;

    public int ToleranceSeconds { get; set; } = Stripe.EventUtility.DefaultTimeTolerance;
}

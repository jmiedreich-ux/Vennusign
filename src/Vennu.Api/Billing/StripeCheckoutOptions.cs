namespace Vennu.Api.Billing;

public sealed class StripeCheckoutOptions
{
    public const string SectionName = "Stripe:Checkout";

    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

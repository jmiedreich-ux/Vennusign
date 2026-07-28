namespace Vennu.Api.Billing;

public sealed class StripeRevenueOptions
{
    public const string SectionName = "Stripe:Revenue";

    public string ApiKey { get; set; } = string.Empty;
}

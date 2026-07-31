namespace Vennu.Api.Billing;

public sealed class StripeBillingPortalOptions
{
    public const string SectionName = "Stripe:BillingPortal";

    public string ReturnUrl { get; set; } = string.Empty;
}

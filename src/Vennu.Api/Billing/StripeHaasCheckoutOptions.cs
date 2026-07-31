namespace Vennu.Api.Billing;

public sealed class StripeHaasCheckoutOptions
{
    public const string SectionName = "Stripe:HaasCheckout";

    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public Dictionary<string, string> PriceIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

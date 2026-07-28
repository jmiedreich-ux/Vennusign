namespace Vennu.Data.Services;

public sealed record BillingCatalogItem(
    Guid TierId,
    string Name,
    string Slug,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    int MaxScreens,
    string? StripeProductId,
    string? StripeMonthlyPriceId,
    string? StripeAnnualPriceId)
{
    public bool IsStripeConfigured =>
        !string.IsNullOrWhiteSpace(StripeProductId) &&
        !string.IsNullOrWhiteSpace(StripeMonthlyPriceId) &&
        !string.IsNullOrWhiteSpace(StripeAnnualPriceId);
}

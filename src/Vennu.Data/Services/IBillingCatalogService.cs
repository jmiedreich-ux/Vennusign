namespace Vennu.Data.Services;

public interface IBillingCatalogService
{
    Task<IReadOnlyCollection<BillingCatalogItem>> GetPublicCatalogAsync(CancellationToken cancellationToken = default);

    Task<BillingCatalogItem> ConfigureStripeAsync(
        Guid tierId,
        string productId,
        string monthlyPriceId,
        string annualPriceId,
        CancellationToken cancellationToken = default);
}

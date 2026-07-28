using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class SubscriptionTierRepository : ISubscriptionTierRepository, IBillingCatalogRepository
{
    private const string ByStripePriceSql = """
        SELECT TOP (1) *
        FROM dbo.SubscriptionTiers
        WHERE StripeMonthlyPriceId = @PriceId
           OR StripeAnnualPriceId = @PriceId;
        """;

    private readonly ISqlDataAccess dataAccess;

    public SubscriptionTierRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await dataAccess.QueryAllAsync<SubscriptionTier>(cancellationToken).ConfigureAwait(false)).ToArray();

    public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        return dataAccess.QueryAsync<SubscriptionTier>(new { Slug = slug }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default) =>
        (await dataAccess.QueryAsync<TierFeature, object>(new { TierId = tierId }, cancellationToken).ConfigureAwait(false)).ToArray();

    public Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default) =>
        dataAccess.QueryAsync<SubscriptionTier>(new { Id = tierId }, cancellationToken);

    public Task<SubscriptionTier?> GetByStripeProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        return dataAccess.QueryAsync<SubscriptionTier>(new { StripeProductId = productId }, cancellationToken);
    }

    public async Task<SubscriptionTier?> GetByStripePriceIdAsync(string priceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(priceId);
        return (await dataAccess.ExecuteSqlQueryAsync<SubscriptionTier, object>(
            ByStripePriceSql,
            new { PriceId = priceId },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
    }

    public async Task<bool> SaveAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) =>
        await dataAccess.UpdateAsync(tier, cancellationToken).ConfigureAwait(false) > 0;
}

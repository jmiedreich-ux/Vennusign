using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IBillingCatalogRepository
{
    Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> GetByStripeProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> GetByStripePriceIdAsync(string priceId, CancellationToken cancellationToken = default);
    Task<bool> SaveAsync(SubscriptionTier tier, CancellationToken cancellationToken = default);
}

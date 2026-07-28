using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ISubscriptionTierRepository
{
    Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default);
    Task<bool> CreateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default);
}

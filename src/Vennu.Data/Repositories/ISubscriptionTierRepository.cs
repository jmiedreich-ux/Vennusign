using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ISubscriptionTierRepository
{
    Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default);
}

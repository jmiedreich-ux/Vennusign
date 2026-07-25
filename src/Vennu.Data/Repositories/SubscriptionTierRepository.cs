using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class SubscriptionTierRepository : ISubscriptionTierRepository
{
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
}

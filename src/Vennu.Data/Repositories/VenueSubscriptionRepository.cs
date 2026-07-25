using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class VenueSubscriptionRepository : IVenueSubscriptionRepository
{
    private readonly ISqlDataAccess dataAccess;

    public VenueSubscriptionRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        dataAccess.QueryAsync<VenueSubscription>(new { VenueId = venueId }, cancellationToken);

    public async Task<bool> SaveAsync(VenueSubscription subscription, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        var utcNow = DateTime.UtcNow;
        subscription.CreatedUtc = subscription.CreatedUtc == default ? utcNow : subscription.CreatedUtc;
        subscription.UpdatedUtc = utcNow;

        var existing = await GetByVenueIdAsync(subscription.VenueId, cancellationToken).ConfigureAwait(false);
        var affected = existing is null
            ? await dataAccess.InsertAsync(subscription, cancellationToken).ConfigureAwait(false)
            : await dataAccess.UpdateAsync(subscription, cancellationToken).ConfigureAwait(false);

        return affected > 0;
    }
}

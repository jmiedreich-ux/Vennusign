using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public interface IOrganizationSubscriptionProjectionService
{
    Task SyncAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default);
    Task<VenueSubscription> SyncVenueAsync(Guid venueId, OrganizationSubscription subscription, CancellationToken cancellationToken = default);
}

public sealed class OrganizationSubscriptionProjectionService(
    IVenueRepository venues,
    IVenueSubscriptionRepository venueSubscriptions,
    IFeatureResolutionService features) : IOrganizationSubscriptionProjectionService
{
    public async Task SyncAsync(
        OrganizationSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        var organizationVenues = (await venues.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .Where(venue => venue.OrganizationId == subscription.OrganizationId)
            .ToArray();
        foreach (var venue in organizationVenues)
            await SyncVenueAsync(venue.Id, subscription, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VenueSubscription> SyncVenueAsync(
        Guid venueId,
        OrganizationSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        var existing = await venueSubscriptions.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var projection = existing ?? new VenueSubscription { VenueId = venueId, CreatedUtc = subscription.CreatedUtc };
        projection.TierId = subscription.TierId;
        projection.StripeSubscriptionId = subscription.StripeSubscriptionId;
        projection.Status = subscription.Status;
        projection.TrialEndsAt = subscription.TrialEndsAt;
        projection.CurrentPeriodEnd = subscription.CurrentPeriodEnd;
        projection.CancelAtPeriodEnd = subscription.CancelAtPeriodEnd;
        projection.UpdatedUtc = subscription.UpdatedUtc;
        if (!await venueSubscriptions.SaveAsync(projection, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("The venue subscription compatibility projection could not be persisted.");
        features.Invalidate(venueId);
        return projection;
    }
}

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
            .OrderBy(venue => venue.Id)
            .ToArray();
        var existing = await venueSubscriptions.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var stripeOwnerVenueId = organizationVenues
            .Select(venue => existing.FirstOrDefault(item =>
                item.VenueId == venue.Id &&
                string.Equals(item.StripeSubscriptionId, subscription.StripeSubscriptionId, StringComparison.Ordinal)))
            .FirstOrDefault(item => item is not null)?.VenueId ?? organizationVenues.FirstOrDefault()?.Id;
        foreach (var venue in organizationVenues)
            await SyncVenueCoreAsync(
                venue.Id,
                subscription,
                venue.Id == stripeOwnerVenueId,
                cancellationToken).ConfigureAwait(false);
    }

    public async Task<VenueSubscription> SyncVenueAsync(
        Guid venueId,
        OrganizationSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        var organizationVenueIds = (await venues.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .Where(venue => venue.OrganizationId == subscription.OrganizationId)
            .Select(venue => venue.Id)
            .ToHashSet();
        var stripeOwner = (await venueSubscriptions.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(item =>
                organizationVenueIds.Contains(item.VenueId) &&
                string.Equals(item.StripeSubscriptionId, subscription.StripeSubscriptionId, StringComparison.Ordinal));
        return await SyncVenueCoreAsync(
            venueId,
            subscription,
            stripeOwner is null || stripeOwner.VenueId == venueId,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<VenueSubscription> SyncVenueCoreAsync(
        Guid venueId,
        OrganizationSubscription subscription,
        bool includeLegacyStripeId,
        CancellationToken cancellationToken)
    {
        var existing = await venueSubscriptions.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var projection = existing ?? new VenueSubscription { VenueId = venueId, CreatedUtc = subscription.CreatedUtc };
        projection.TierId = subscription.TierId;
        projection.StripeSubscriptionId = includeLegacyStripeId ? subscription.StripeSubscriptionId : null;
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

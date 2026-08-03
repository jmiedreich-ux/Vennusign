using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class TierScreenLimitReachedException : InvalidOperationException
{
    public TierScreenLimitReachedException() : base("The tier screen limit has been reached.") { }
}

public interface IVenueEntitlementService
{
    Task EnsureCanAddScreenAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task EnsureCanAddVenueAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

public sealed class VenueEntitlementService(
    IVenueSubscriptionRepository subscriptions,
    ISubscriptionTierRepository tiers,
    IScreenRepository screens,
    TimeProvider timeProvider,
    IVenueRepository? venues = null,
    IOrganizationSubscriptionRepository? organizationSubscriptions = null) : IVenueEntitlementService
{
    public async Task EnsureCanAddScreenAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Venue ID is required.", nameof(venueId));
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var venue = venues is null ? null : await venues.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var organizationSubscription = venue?.OrganizationId is Guid organizationId && organizationSubscriptions is not null
            ? await organizationSubscriptions.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            : null;
        var legacySubscription = organizationSubscription is null
            ? await subscriptions.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            : null;
        var status = organizationSubscription?.Status ?? legacySubscription?.Status
            ?? throw new InvalidOperationException("An authoritative commercial subscription is required.");
        var trialEndsAt = organizationSubscription?.TrialEndsAt ?? legacySubscription?.TrialEndsAt;
        var tierId = organizationSubscription?.TierId ?? legacySubscription!.TierId;
        var entitled = status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
            status.Equals("trialing", StringComparison.OrdinalIgnoreCase) && trialEndsAt > utcNow;
        if (!entitled) throw new InvalidOperationException("The venue does not have an active entitlement.");
        var tier = await tiers.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The subscribed tier no longer exists.");
        if (tier.MaxScreens < 0) return;
        var current = await screens.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (current.Count >= tier.MaxScreens) throw new TierScreenLimitReachedException();
    }

    public async Task EnsureCanAddVenueAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("Organization ID is required.", nameof(organizationId));
        if (organizationSubscriptions is null || venues is null)
            throw new InvalidOperationException("Organization entitlement services are unavailable.");
        var subscription = await organizationSubscriptions.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("An authoritative organization subscription is required.");
        OrganizationSubscriptionManagementService.EnsureEntitled(subscription, timeProvider.GetUtcNow().UtcDateTime);
        var tier = await tiers.GetByIdAsync(subscription.TierId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The subscribed tier no longer exists.");
        if (tier.MaxVenues < 0) return;
        var current = await venues.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (current.Count(item => item.OrganizationId == organizationId) >= tier.MaxVenues)
            throw new InvalidOperationException("The tier venue limit has been reached.");
    }
}

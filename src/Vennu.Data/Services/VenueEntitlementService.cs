using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public interface IVenueEntitlementService
{
    Task EnsureCanAddScreenAsync(Guid venueId, CancellationToken cancellationToken = default);
}

public sealed class VenueEntitlementService(
    IVenueSubscriptionRepository subscriptions,
    ISubscriptionTierRepository tiers,
    IScreenRepository screens,
    TimeProvider timeProvider) : IVenueEntitlementService
{
    public async Task EnsureCanAddScreenAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Venue ID is required.", nameof(venueId));
        var subscription = await subscriptions.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("An authoritative venue subscription is required.");
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var entitled = subscription.Status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
            subscription.Status.Equals("trialing", StringComparison.OrdinalIgnoreCase) && subscription.TrialEndsAt > utcNow;
        if (!entitled) throw new InvalidOperationException("The venue does not have an active entitlement.");
        var tier = await tiers.GetByIdAsync(subscription.TierId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The subscribed tier no longer exists.");
        if (tier.MaxScreens < 0) return;
        var current = await screens.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (current.Count >= tier.MaxScreens) throw new InvalidOperationException("The tier screen limit has been reached.");
    }
}

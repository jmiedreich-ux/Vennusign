using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public interface IOrganizationSubscriptionManagementService
{
    Task<OrganizationSubscription> StartTrialAsync(Guid organizationId, Guid tierId, CancellationToken cancellationToken = default);
    Task EnsureCanAddVenueAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

public sealed class OrganizationSubscriptionManagementService(
    IOrganizationSubscriptionRepository subscriptions,
    ISubscriptionTierRepository tiers,
    IVenueRepository venues,
    IOrganizationSubscriptionProjectionService projections,
    TimeProvider timeProvider) : IOrganizationSubscriptionManagementService
{
    public async Task<OrganizationSubscription> StartTrialAsync(
        Guid organizationId,
        Guid tierId,
        CancellationToken cancellationToken = default)
    {
        RequireId(organizationId, nameof(organizationId));
        RequireId(tierId, nameof(tierId));
        if (await subscriptions.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false) is not null)
            throw new InvalidOperationException("A commercial subscription already exists for this organization.");

        var tier = await tiers.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The subscription tier does not exist.");
        if (!tier.IsActive || !tier.IsPublic || tier.TrialDays <= 0)
            throw new InvalidOperationException("The selected tier does not offer a no-card trial.");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var subscription = new OrganizationSubscription
        {
            OrganizationId = organizationId,
            TierId = tierId,
            Status = "trialing",
            TrialEndsAt = utcNow.AddDays(tier.TrialDays),
            CreatedUtc = utcNow,
            UpdatedUtc = utcNow
        };
        if (!await subscriptions.SaveAsync(subscription, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("The organization subscription could not be persisted.");
        await projections.SyncAsync(subscription, cancellationToken).ConfigureAwait(false);
        return subscription;
    }

    public async Task EnsureCanAddVenueAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        RequireId(organizationId, nameof(organizationId));
        var subscription = await subscriptions.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("An authoritative organization subscription is required.");
        EnsureEntitled(subscription, timeProvider.GetUtcNow().UtcDateTime);
        var tier = await tiers.GetByIdAsync(subscription.TierId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The subscribed tier no longer exists.");
        if (tier.MaxVenues < 0) return;
        var current = await venues.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (current.Count(venue => venue.OrganizationId == organizationId) >= tier.MaxVenues)
            throw new InvalidOperationException("The tier venue limit has been reached.");
    }

    internal static void EnsureEntitled(OrganizationSubscription subscription, DateTime utcNow)
    {
        var entitled = subscription.Status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
            subscription.Status.Equals("trialing", StringComparison.OrdinalIgnoreCase) && subscription.TrialEndsAt > utcNow;
        if (!entitled)
            throw new InvalidOperationException("The organization does not have an active entitlement.");
    }

    private static void RequireId(Guid value, string name)
    {
        if (value == Guid.Empty) throw new ArgumentException("A non-empty ID is required.", name);
    }
}

using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class VenueTierSwitchService(
    IVenueRepository venueRepository,
    IVenueSubscriptionRepository subscriptionRepository,
    ISubscriptionTierRepository tierRepository,
    IStripeSubscriptionTierUpdater stripeUpdater,
    IOperationalEventRepository eventRepository,
    IFeatureResolutionService featureResolutionService,
    TimeProvider timeProvider,
    IOrganizationSubscriptionRepository? organizationSubscriptionRepository = null,
    IOrganizationSubscriptionProjectionService? projectionService = null) : IVenueTierSwitchService
{
    public async Task<VenueSubscription> SwitchAsync(
        Guid venueId,
        Guid targetTierId,
        CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Venue ID is required.", nameof(venueId));
        if (targetTierId == Guid.Empty) throw new ArgumentException("Target tier ID is required.", nameof(targetTierId));
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Venue '{venueId}' was not found.");
        var targetTier = await tierRepository.GetByIdAsync(targetTierId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Tier '{targetTierId}' was not found.");
        if (!targetTier.IsActive) throw new InvalidOperationException("The target tier is archived.");
        if (string.IsNullOrWhiteSpace(targetTier.StripeMonthlyPriceId))
            throw new InvalidOperationException("The target tier does not have a Stripe monthly price mapping.");

        var organizationSubscription = venue.OrganizationId is Guid organizationId && organizationSubscriptionRepository is not null
            ? await organizationSubscriptionRepository.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            : null;
        var legacySubscription = organizationSubscription is null
            ? await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"Venue '{venueId}' does not have a subscription.")
            : null;
        var currentTierId = organizationSubscription?.TierId ?? legacySubscription!.TierId;
        var stripeSubscriptionId = organizationSubscription?.StripeSubscriptionId ?? legacySubscription?.StripeSubscriptionId;
        if (currentTierId == targetTierId)
            return organizationSubscription is null
                ? legacySubscription!
                : await RequireProjectionAsync(venueId, organizationSubscription, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            throw new InvalidOperationException("The organization subscription is not linked to Stripe.");

        var currentTier = await tierRepository.GetByIdAsync(currentTierId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The current tier could not be resolved.");
        var change = await stripeUpdater.ChangeAsync(
            stripeSubscriptionId,
            targetTier.StripeMonthlyPriceId,
            targetTier.StripeAnnualPriceId,
            cancellationToken).ConfigureAwait(false);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            if (organizationSubscription is not null)
            {
                organizationSubscription.TierId = targetTier.Id;
                organizationSubscription.UpdatedUtc = utcNow;
                if (!await organizationSubscriptionRepository!.SaveAsync(organizationSubscription, cancellationToken).ConfigureAwait(false))
                    throw new InvalidOperationException("The local organization subscription could not be persisted.");
                await projectionService!.SyncAsync(organizationSubscription, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                legacySubscription!.TierId = targetTier.Id;
                legacySubscription.UpdatedUtc = utcNow;
                if (!await subscriptionRepository.SaveAsync(legacySubscription, cancellationToken).ConfigureAwait(false))
                    throw new InvalidOperationException("The local venue subscription could not be persisted.");
            }
            var eventType = targetTier.Price < currentTier.Price ? "downgrade" : "upgrade";
            await eventRepository.AddAsync(new OperationalEvent
            {
                Id = Guid.NewGuid(), VenueId = venueId, EventType = eventType,
                Summary = $"{(eventType == "upgrade" ? "Upgraded" : "Downgraded")} to {targetTier.Name}", OccurredUtc = utcNow
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var restored = false;
            if (organizationSubscription is not null)
            {
                organizationSubscription.TierId = currentTierId;
                organizationSubscription.UpdatedUtc = utcNow;
                restored = await organizationSubscriptionRepository!.SaveAsync(organizationSubscription, cancellationToken).ConfigureAwait(false);
                if (restored) await projectionService!.SyncAsync(organizationSubscription, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                legacySubscription!.TierId = currentTierId;
                legacySubscription.UpdatedUtc = utcNow;
                restored = await subscriptionRepository.SaveAsync(legacySubscription, cancellationToken).ConfigureAwait(false);
            }
            await stripeUpdater.RestoreAsync(change, cancellationToken).ConfigureAwait(false);
            featureResolutionService.Invalidate(venueId);
            if (!restored)
                throw new InvalidOperationException("The tier switch failed and local subscription restoration also failed; manual reconciliation is required.", exception);
            throw new InvalidOperationException("The tier switch failed and was restored.", exception);
        }

        featureResolutionService.Invalidate(venueId);
        return organizationSubscription is null
            ? legacySubscription!
            : await RequireProjectionAsync(venueId, organizationSubscription, cancellationToken).ConfigureAwait(false);
    }

    private async Task<VenueSubscription> RequireProjectionAsync(
        Guid venueId,
        OrganizationSubscription subscription,
        CancellationToken cancellationToken)
    {
        if (projectionService is null)
            throw new InvalidOperationException("Organization subscription projection is unavailable.");
        return await projectionService.SyncVenueAsync(venueId, subscription, cancellationToken).ConfigureAwait(false);
    }
}

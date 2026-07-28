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
    TimeProvider timeProvider) : IVenueTierSwitchService
{
    public async Task<VenueSubscription> SwitchAsync(
        Guid venueId,
        Guid targetTierId,
        CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Venue ID is required.", nameof(venueId));
        if (targetTierId == Guid.Empty) throw new ArgumentException("Target tier ID is required.", nameof(targetTierId));

        var venueTask = venueRepository.GetByIdAsync(venueId, cancellationToken);
        var subscriptionTask = subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken);
        var targetTierTask = tierRepository.GetByIdAsync(targetTierId, cancellationToken);
        await Task.WhenAll(venueTask, subscriptionTask, targetTierTask).ConfigureAwait(false);

        _ = venueTask.Result ?? throw new KeyNotFoundException($"Venue '{venueId}' was not found.");
        var subscription = subscriptionTask.Result
            ?? throw new KeyNotFoundException($"Venue '{venueId}' does not have a subscription.");
        var targetTier = targetTierTask.Result
            ?? throw new KeyNotFoundException($"Tier '{targetTierId}' was not found.");

        if (!targetTier.IsActive) throw new InvalidOperationException("The target tier is archived.");
        if (string.IsNullOrWhiteSpace(targetTier.StripeMonthlyPriceId))
        {
            throw new InvalidOperationException("The target tier does not have a Stripe monthly price mapping.");
        }

        if (subscription.TierId == targetTierId) return subscription;
        if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
        {
            throw new InvalidOperationException("The venue subscription is not linked to Stripe.");
        }

        var currentTier = await tierRepository.GetByIdAsync(subscription.TierId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The venue's current tier could not be resolved.");
        var change = await stripeUpdater.ChangeAsync(
            subscription.StripeSubscriptionId,
            targetTier.StripeMonthlyPriceId,
            targetTier.StripeAnnualPriceId,
            cancellationToken).ConfigureAwait(false);

        var previousTierId = subscription.TierId;
        var previousUpdatedUtc = subscription.UpdatedUtc;
        subscription.TierId = targetTier.Id;
        subscription.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            if (!await subscriptionRepository.SaveAsync(subscription, cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException("The local subscription could not be persisted.");
            }

            var eventType = targetTier.Price < currentTier.Price ? "downgrade" : "upgrade";
            await eventRepository.AddAsync(
                new OperationalEvent
                {
                    Id = Guid.NewGuid(),
                    VenueId = venueId,
                    EventType = eventType,
                    Summary = $"{(eventType == "upgrade" ? "Upgraded" : "Downgraded")} to {targetTier.Name}",
                    OccurredUtc = subscription.UpdatedUtc
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            subscription.TierId = previousTierId;
            subscription.UpdatedUtc = previousUpdatedUtc;
            var localRestored = await subscriptionRepository.SaveAsync(subscription, cancellationToken).ConfigureAwait(false);
            await stripeUpdater.RestoreAsync(change, cancellationToken).ConfigureAwait(false);
            featureResolutionService.Invalidate(venueId);
            if (!localRestored)
            {
                throw new InvalidOperationException(
                    "The tier switch failed and local subscription restoration also failed; manual reconciliation is required.",
                    exception);
            }

            throw new InvalidOperationException("The tier switch failed and was restored.", exception);
        }

        featureResolutionService.Invalidate(venueId);
        return subscription;
    }
}

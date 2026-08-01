using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class SubscriptionManagementService : ISubscriptionManagementService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "trialing",
        "active",
        "past_due",
        "canceled"
    };

    private readonly IVenueSubscriptionRepository subscriptionRepository;
    private readonly IFeatureResolutionService featureResolutionService;
    private readonly TimeProvider timeProvider;
    private readonly ISubscriptionTierRepository tierRepository;

    public SubscriptionManagementService(
        IVenueSubscriptionRepository subscriptionRepository,
        IFeatureResolutionService featureResolutionService,
        ISubscriptionTierRepository tierRepository,
        TimeProvider timeProvider)
    {
        this.subscriptionRepository = subscriptionRepository;
        this.featureResolutionService = featureResolutionService;
        this.tierRepository = tierRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<VenueSubscription> StartTrialAsync(
        Guid venueId,
        Guid tierId,
        CancellationToken cancellationToken = default)
    {
        ValidateIds(venueId, tierId);
        var existing = await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            throw new InvalidOperationException("A subscription already exists for this venue.");
        }

        var tier = await tierRepository.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The subscription tier does not exist.");
        if (!tier.IsActive || !tier.IsPublic || tier.TrialDays <= 0)
            throw new InvalidOperationException("The selected tier does not offer a no-card trial.");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var subscription = new VenueSubscription
        {
            VenueId = venueId,
            TierId = tierId,
            Status = "trialing",
            TrialEndsAt = utcNow.AddDays(tier.TrialDays),
            CreatedUtc = utcNow,
            UpdatedUtc = utcNow
        };

        await SaveAndInvalidateAsync(subscription, cancellationToken).ConfigureAwait(false);
        return subscription;
    }

    public async Task<VenueSubscription> ChangeTierAsync(
        Guid venueId,
        Guid tierId,
        CancellationToken cancellationToken = default)
    {
        ValidateIds(venueId, tierId);
        var subscription = await GetRequiredAsync(venueId, cancellationToken).ConfigureAwait(false);
        subscription.TierId = tierId;
        subscription.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;

        await SaveAndInvalidateAsync(subscription, cancellationToken).ConfigureAwait(false);
        return subscription;
    }

    public async Task<VenueSubscription> SetStatusAsync(
        Guid venueId,
        string status,
        DateTime? currentPeriodEnd = null,
        CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        var normalizedStatus = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalizedStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported subscription status.");
        }

        var subscription = await GetRequiredAsync(venueId, cancellationToken).ConfigureAwait(false);
        subscription.Status = normalizedStatus;
        subscription.CurrentPeriodEnd = currentPeriodEnd ?? subscription.CurrentPeriodEnd;
        subscription.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;

        if (normalizedStatus == "active")
        {
            subscription.TrialEndsAt = null;
        }

        await SaveAndInvalidateAsync(subscription, cancellationToken).ConfigureAwait(false);
        return subscription;
    }

    public async Task<int> ExpireTrialsAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var subscriptions = await subscriptionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var expired = subscriptions
            .Where(subscription =>
                subscription.Status.Equals("trialing", StringComparison.OrdinalIgnoreCase) &&
                subscription.TrialEndsAt is not null &&
                subscription.TrialEndsAt <= utcNow)
            .ToArray();

        foreach (var subscription in expired)
        {
            subscription.Status = "canceled";
            subscription.UpdatedUtc = utcNow;
            await SaveAndInvalidateAsync(subscription, cancellationToken).ConfigureAwait(false);
        }

        return expired.Length;
    }

    private async Task<VenueSubscription> GetRequiredAsync(Guid venueId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        return subscription ?? throw new KeyNotFoundException($"No subscription exists for venue '{venueId}'.");
    }

    private async Task SaveAndInvalidateAsync(VenueSubscription subscription, CancellationToken cancellationToken)
    {
        if (!await subscriptionRepository.SaveAsync(subscription, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The venue subscription could not be persisted.");
        }

        featureResolutionService.Invalidate(subscription.VenueId);
    }

    private static void ValidateIds(Guid venueId, Guid tierId)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        if (tierId == Guid.Empty)
        {
            throw new ArgumentException("Tier ID is required.", nameof(tierId));
        }
    }
}

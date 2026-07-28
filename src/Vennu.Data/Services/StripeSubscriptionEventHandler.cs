using System.Security.Cryptography;
using System.Text;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class StripeSubscriptionEventHandler : IStripeSubscriptionEventHandler
{
    private const string SubscriptionCreated = "subscription.created";
    private const string CustomerSubscriptionCreated = "customer.subscription.created";
    private const string CustomerSubscriptionUpdated = "customer.subscription.updated";
    private const string InvoicePaid = "invoice.paid";
    private const string CustomerSubscriptionDeleted = "customer.subscription.deleted";
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        "trialing",
        "active",
        "past_due",
        "canceled"
    };

    private readonly IStripeEventIdempotencyService idempotencyService;
    private readonly IBillingCatalogRepository billingCatalogRepository;
    private readonly IVenueSubscriptionRepository subscriptionRepository;
    private readonly IOperationalEventRepository operationalEventRepository;
    private readonly IFeatureResolutionService featureResolutionService;
    private readonly TimeProvider timeProvider;

    public StripeSubscriptionEventHandler(
        IStripeEventIdempotencyService idempotencyService,
        IBillingCatalogRepository billingCatalogRepository,
        IVenueSubscriptionRepository subscriptionRepository,
        IOperationalEventRepository operationalEventRepository,
        IFeatureResolutionService featureResolutionService,
        TimeProvider timeProvider)
    {
        this.idempotencyService = idempotencyService;
        this.billingCatalogRepository = billingCatalogRepository;
        this.subscriptionRepository = subscriptionRepository;
        this.operationalEventRepository = operationalEventRepository;
        this.featureResolutionService = featureResolutionService;
        this.timeProvider = timeProvider;
    }

    public Task<bool> HandleAsync(
        StripeSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stripeEvent);
        var eventType = Normalize(stripeEvent.EventType, nameof(stripeEvent.EventType));
        if (!IsSupported(eventType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(stripeEvent),
                stripeEvent.EventType,
                "Unsupported Stripe subscription event type.");
        }

        return idempotencyService.ExecuteOnceAsync(
            stripeEvent.EventId,
            eventType,
            token => ApplyAsync(stripeEvent with { EventType = eventType }, token),
            cancellationToken);
    }

    private Task ApplyAsync(StripeSubscriptionEvent stripeEvent, CancellationToken cancellationToken) =>
        stripeEvent.EventType switch
        {
            SubscriptionCreated or CustomerSubscriptionCreated or CustomerSubscriptionUpdated =>
                ApplySubscriptionAsync(stripeEvent, cancellationToken),
            InvoicePaid => ApplyInvoicePaidAsync(stripeEvent, cancellationToken),
            CustomerSubscriptionDeleted => ApplyDeletedAsync(stripeEvent, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(stripeEvent), stripeEvent.EventType, "Unsupported Stripe event.")
        };

    private async Task ApplySubscriptionAsync(
        StripeSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken)
    {
        var stripeSubscriptionId = Normalize(stripeEvent.StripeSubscriptionId, nameof(stripeEvent.StripeSubscriptionId));
        var stripePriceId = Normalize(stripeEvent.StripePriceId, nameof(stripeEvent.StripePriceId));
        var status = NormalizeStatus(stripeEvent.Status);
        if (stripeEvent.VenueId is null || stripeEvent.VenueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required for subscription create and update events.", nameof(stripeEvent));
        }

        var tier = await billingCatalogRepository.GetByStripePriceIdAsync(stripePriceId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"No subscription tier is configured for Stripe price '{stripePriceId}'.");
        var existingByStripe = await subscriptionRepository.GetByStripeSubscriptionIdAsync(
            stripeSubscriptionId,
            cancellationToken).ConfigureAwait(false);
        var existingByVenue = await subscriptionRepository.GetByVenueIdAsync(
            stripeEvent.VenueId.Value,
            cancellationToken).ConfigureAwait(false);
        var subscription = existingByStripe ?? existingByVenue;
        var isNewSubscription = subscription is null;
        var previousTierId = subscription?.TierId;

        if (existingByStripe is not null && existingByStripe.VenueId != stripeEvent.VenueId.Value)
        {
            throw new InvalidOperationException("The Stripe subscription is already assigned to another venue.");
        }

        if (existingByVenue?.StripeSubscriptionId is not null &&
            !existingByVenue.StripeSubscriptionId.Equals(stripeSubscriptionId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The venue is already assigned to another Stripe subscription.");
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        subscription ??= new VenueSubscription
        {
            VenueId = stripeEvent.VenueId.Value,
            CreatedUtc = utcNow
        };
        subscription.TierId = tier.Id;
        subscription.StripeSubscriptionId = stripeSubscriptionId;
        subscription.Status = status;
        subscription.TrialEndsAt = status == "active" ? null : stripeEvent.TrialEndsAt;
        subscription.CurrentPeriodEnd = stripeEvent.CurrentPeriodEnd ?? subscription.CurrentPeriodEnd;
        subscription.UpdatedUtc = utcNow;
        await SaveAndInvalidateAsync(subscription, cancellationToken).ConfigureAwait(false);

        if (isNewSubscription)
        {
            await RecordAsync(
                stripeEvent.EventId,
                subscription.VenueId,
                "signup",
                $"New {tier.Name} subscription",
                utcNow,
                cancellationToken).ConfigureAwait(false);
        }
        else if (previousTierId != tier.Id)
        {
            var previousTier = previousTierId is null
                ? null
                : await billingCatalogRepository.GetByIdAsync(previousTierId.Value, cancellationToken).ConfigureAwait(false);
            var eventType = previousTier is not null && tier.Price < previousTier.Price ? "downgrade" : "upgrade";
            await RecordAsync(
                stripeEvent.EventId,
                subscription.VenueId,
                eventType,
                $"{(eventType == "upgrade" ? "Upgraded" : "Downgraded")} to {tier.Name}",
                utcNow,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ApplyInvoicePaidAsync(
        StripeSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken)
    {
        var subscription = await GetRequiredSubscriptionAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
        if (stripeEvent.CurrentPeriodEnd is null)
        {
            throw new ArgumentException("Current period end is required for invoice.paid.", nameof(stripeEvent));
        }

        subscription.Status = "active";
        subscription.TrialEndsAt = null;
        subscription.CurrentPeriodEnd = stripeEvent.CurrentPeriodEnd;
        subscription.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await SaveAndInvalidateAsync(subscription, cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyDeletedAsync(
        StripeSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken)
    {
        var subscription = await GetRequiredSubscriptionAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
        subscription.Status = "canceled";
        subscription.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await SaveAndInvalidateAsync(subscription, cancellationToken).ConfigureAwait(false);
        await RecordAsync(
            stripeEvent.EventId,
            subscription.VenueId,
            "churn",
            "Subscription canceled",
            subscription.UpdatedUtc,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<VenueSubscription> GetRequiredSubscriptionAsync(
        StripeSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken)
    {
        var stripeSubscriptionId = Normalize(stripeEvent.StripeSubscriptionId, nameof(stripeEvent.StripeSubscriptionId));
        return await subscriptionRepository.GetByStripeSubscriptionIdAsync(
            stripeSubscriptionId,
            cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Stripe subscription '{stripeSubscriptionId}' is not assigned to a venue.");
    }

    private async Task SaveAndInvalidateAsync(
        VenueSubscription subscription,
        CancellationToken cancellationToken)
    {
        if (!await subscriptionRepository.SaveAsync(subscription, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The venue subscription could not be persisted.");
        }

        featureResolutionService.Invalidate(subscription.VenueId);
    }

    private Task RecordAsync(
        string sourceEventId,
        Guid venueId,
        string eventType,
        string summary,
        DateTime occurredUtc,
        CancellationToken cancellationToken) =>
        operationalEventRepository.AddAsync(
            new OperationalEvent
            {
                Id = CreateOperationalEventId(sourceEventId),
                VenueId = venueId,
                EventType = eventType,
                Summary = summary,
                OccurredUtc = occurredUtc
            },
            cancellationToken);

    private static Guid CreateOperationalEventId(string sourceEventId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sourceEventId));
        return new Guid(hash.AsSpan(0, 16));
    }

    private static bool IsSupported(string eventType) =>
        eventType is SubscriptionCreated or CustomerSubscriptionCreated or CustomerSubscriptionUpdated or InvoicePaid or CustomerSubscriptionDeleted;

    private static string NormalizeStatus(string? status)
    {
        var normalized = Normalize(status, nameof(status)).ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported subscription status.");
        }

        return normalized;
    }

    private static string Normalize(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}

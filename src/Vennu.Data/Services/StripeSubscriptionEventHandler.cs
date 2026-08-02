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
        "trialing", "active", "past_due", "canceled"
    };

    private readonly IStripeEventIdempotencyService idempotencyService;
    private readonly IBillingCatalogRepository billingCatalogRepository;
    private readonly IVenueSubscriptionRepository venueSubscriptionRepository;
    private readonly IOperationalEventRepository operationalEventRepository;
    private readonly IFeatureResolutionService featureResolutionService;
    private readonly TimeProvider timeProvider;
    private readonly IOrganizationSubscriptionRepository? organizationSubscriptionRepository;
    private readonly IVenueRepository? venueRepository;
    private readonly IOrganizationSubscriptionProjectionService? projectionService;

    public StripeSubscriptionEventHandler(
        IStripeEventIdempotencyService idempotencyService,
        IBillingCatalogRepository billingCatalogRepository,
        IVenueSubscriptionRepository subscriptionRepository,
        IOperationalEventRepository operationalEventRepository,
        IFeatureResolutionService featureResolutionService,
        TimeProvider timeProvider,
        IOrganizationSubscriptionRepository? organizationSubscriptionRepository = null,
        IVenueRepository? venueRepository = null,
        IOrganizationSubscriptionProjectionService? projectionService = null)
    {
        this.idempotencyService = idempotencyService;
        this.billingCatalogRepository = billingCatalogRepository;
        venueSubscriptionRepository = subscriptionRepository;
        this.operationalEventRepository = operationalEventRepository;
        this.featureResolutionService = featureResolutionService;
        this.timeProvider = timeProvider;
        this.organizationSubscriptionRepository = organizationSubscriptionRepository;
        this.venueRepository = venueRepository;
        this.projectionService = projectionService;
    }

    public Task<bool> HandleAsync(StripeSubscriptionEvent stripeEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stripeEvent);
        var eventType = Normalize(stripeEvent.EventType, nameof(stripeEvent.EventType));
        if (!IsSupported(eventType))
            throw new ArgumentOutOfRangeException(nameof(stripeEvent), stripeEvent.EventType, "Unsupported Stripe subscription event type.");
        return idempotencyService.ExecuteOnceAsync(
            stripeEvent.EventId,
            eventType,
            token => ApplyAsync(stripeEvent with { EventType = eventType }, token),
            cancellationToken);
    }

    private Task ApplyAsync(StripeSubscriptionEvent stripeEvent, CancellationToken cancellationToken) =>
        stripeEvent.EventType switch
        {
            SubscriptionCreated or CustomerSubscriptionCreated or CustomerSubscriptionUpdated => ApplySubscriptionAsync(stripeEvent, cancellationToken),
            InvoicePaid => ApplyInvoicePaidAsync(stripeEvent, cancellationToken),
            CustomerSubscriptionDeleted => ApplyDeletedAsync(stripeEvent, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(stripeEvent), stripeEvent.EventType, "Unsupported Stripe event.")
        };

    private async Task ApplySubscriptionAsync(StripeSubscriptionEvent stripeEvent, CancellationToken cancellationToken)
    {
        var stripeSubscriptionId = Normalize(stripeEvent.StripeSubscriptionId, nameof(stripeEvent.StripeSubscriptionId));
        var stripePriceId = Normalize(stripeEvent.StripePriceId, nameof(stripeEvent.StripePriceId));
        var status = NormalizeStatus(stripeEvent.Status);
        var tier = await billingCatalogRepository.GetByStripePriceIdAsync(stripePriceId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"No subscription tier is configured for Stripe price '{stripePriceId}'.");
        var organizationId = await ResolveOrganizationIdAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
        if (organizationId is not null)
        {
            await ApplyOrganizationSubscriptionAsync(stripeEvent, organizationId.Value, stripeSubscriptionId, status, tier, cancellationToken)
                .ConfigureAwait(false);
            return;
        }
        await ApplyLegacyVenueSubscriptionAsync(stripeEvent, stripeSubscriptionId, status, tier, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ApplyOrganizationSubscriptionAsync(
        StripeSubscriptionEvent stripeEvent,
        Guid organizationId,
        string stripeSubscriptionId,
        string status,
        SubscriptionTier tier,
        CancellationToken cancellationToken)
    {
        if (organizationSubscriptionRepository is null || projectionService is null)
            throw new InvalidOperationException("Organization subscription persistence is unavailable.");
        var existingByStripe = await organizationSubscriptionRepository
            .GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        var existingByOrganization = await organizationSubscriptionRepository
            .GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false);
        if (existingByStripe is not null && existingByStripe.OrganizationId != organizationId)
            throw new InvalidOperationException("The Stripe subscription is already assigned to another organization.");
        if (existingByOrganization?.StripeSubscriptionId is not null &&
            !existingByOrganization.StripeSubscriptionId.Equals(stripeSubscriptionId, StringComparison.Ordinal))
            throw new InvalidOperationException("The organization is already assigned to another Stripe subscription.");

        var customerId = NormalizeOptional(stripeEvent.StripeCustomerId);
        if (existingByOrganization?.StripeCustomerId is not null && customerId is not null &&
            !existingByOrganization.StripeCustomerId.Equals(customerId, StringComparison.Ordinal))
            throw new InvalidOperationException("The organization is already assigned to another Stripe customer.");

        var subscription = existingByStripe ?? existingByOrganization;
        var isNew = subscription is null;
        var previousTierId = subscription?.TierId;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        subscription ??= new OrganizationSubscription { OrganizationId = organizationId, CreatedUtc = utcNow };
        subscription.TierId = tier.Id;
        subscription.StripeCustomerId = customerId ?? subscription.StripeCustomerId;
        subscription.StripeSubscriptionId = stripeSubscriptionId;
        subscription.Status = status;
        subscription.TrialEndsAt = status == "active" ? null : stripeEvent.TrialEndsAt;
        subscription.CurrentPeriodEnd = stripeEvent.CurrentPeriodEnd ?? subscription.CurrentPeriodEnd;
        subscription.CancelAtPeriodEnd = stripeEvent.CancelAtPeriodEnd;
        subscription.UpdatedUtc = utcNow;
        await SaveOrganizationAsync(subscription, cancellationToken).ConfigureAwait(false);

        if (stripeEvent.VenueId is Guid venueId)
            await RecordTierEventAsync(stripeEvent.EventId, venueId, isNew, previousTierId, tier, utcNow, cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyLegacyVenueSubscriptionAsync(
        StripeSubscriptionEvent stripeEvent,
        string stripeSubscriptionId,
        string status,
        SubscriptionTier tier,
        CancellationToken cancellationToken)
    {
        if (stripeEvent.VenueId is null || stripeEvent.VenueId == Guid.Empty)
            throw new ArgumentException("Organization or legacy venue metadata is required for subscription create and update events.", nameof(stripeEvent));
        var venueId = stripeEvent.VenueId.Value;
        var existingByStripe = await venueSubscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        var existingByVenue = await venueSubscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (existingByStripe is not null && existingByStripe.VenueId != venueId)
            throw new InvalidOperationException("The Stripe subscription is already assigned to another venue.");
        if (existingByVenue?.StripeSubscriptionId is not null && !existingByVenue.StripeSubscriptionId.Equals(stripeSubscriptionId, StringComparison.Ordinal))
            throw new InvalidOperationException("The venue is already assigned to another Stripe subscription.");

        var subscription = existingByStripe ?? existingByVenue;
        var isNew = subscription is null;
        var previousTierId = subscription?.TierId;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        subscription ??= new VenueSubscription { VenueId = venueId, CreatedUtc = utcNow };
        subscription.TierId = tier.Id;
        subscription.StripeSubscriptionId = stripeSubscriptionId;
        subscription.Status = status;
        subscription.TrialEndsAt = status == "active" ? null : stripeEvent.TrialEndsAt;
        subscription.CurrentPeriodEnd = stripeEvent.CurrentPeriodEnd ?? subscription.CurrentPeriodEnd;
        subscription.CancelAtPeriodEnd = stripeEvent.CancelAtPeriodEnd;
        subscription.UpdatedUtc = utcNow;
        await SaveVenueAsync(subscription, cancellationToken).ConfigureAwait(false);
        await RecordTierEventAsync(stripeEvent.EventId, venueId, isNew, previousTierId, tier, utcNow, cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyInvoicePaidAsync(StripeSubscriptionEvent stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.CurrentPeriodEnd is null)
            throw new ArgumentException("Current period end is required for invoice.paid.", nameof(stripeEvent));
        var stripeSubscriptionId = Normalize(stripeEvent.StripeSubscriptionId, nameof(stripeEvent.StripeSubscriptionId));
        var organization = organizationSubscriptionRepository is null
            ? null
            : await organizationSubscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (organization is not null)
        {
            organization.Status = "active";
            organization.TrialEndsAt = null;
            organization.CurrentPeriodEnd = stripeEvent.CurrentPeriodEnd;
            organization.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
            await SaveOrganizationAsync(organization, cancellationToken).ConfigureAwait(false);
            return;
        }
        var venue = await GetRequiredVenueSubscriptionAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        venue.Status = "active";
        venue.TrialEndsAt = null;
        venue.CurrentPeriodEnd = stripeEvent.CurrentPeriodEnd;
        venue.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await SaveVenueAsync(venue, cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyDeletedAsync(StripeSubscriptionEvent stripeEvent, CancellationToken cancellationToken)
    {
        var stripeSubscriptionId = Normalize(stripeEvent.StripeSubscriptionId, nameof(stripeEvent.StripeSubscriptionId));
        var organization = organizationSubscriptionRepository is null
            ? null
            : await organizationSubscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        if (organization is not null)
        {
            organization.Status = "canceled";
            organization.CancelAtPeriodEnd = false;
            organization.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
            await SaveOrganizationAsync(organization, cancellationToken).ConfigureAwait(false);
            return;
        }
        var venue = await GetRequiredVenueSubscriptionAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        venue.Status = "canceled";
        venue.CancelAtPeriodEnd = false;
        venue.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await SaveVenueAsync(venue, cancellationToken).ConfigureAwait(false);
        await RecordAsync(stripeEvent.EventId, venue.VenueId, "churn", "Subscription canceled", venue.UpdatedUtc, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid?> ResolveOrganizationIdAsync(StripeSubscriptionEvent stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.OrganizationId is Guid organizationId && organizationId != Guid.Empty) return organizationId;
        if (stripeEvent.VenueId is not Guid venueId || venueId == Guid.Empty || venueRepository is null) return null;
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        return venue?.OrganizationId;
    }

    private async Task<VenueSubscription> GetRequiredVenueSubscriptionAsync(string stripeSubscriptionId, CancellationToken cancellationToken) =>
        await venueSubscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false)
        ?? throw new KeyNotFoundException($"Stripe subscription '{stripeSubscriptionId}' is not assigned to an organization or venue.");

    private async Task SaveOrganizationAsync(OrganizationSubscription subscription, CancellationToken cancellationToken)
    {
        if (organizationSubscriptionRepository is null || projectionService is null ||
            !await organizationSubscriptionRepository.SaveAsync(subscription, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("The organization subscription could not be persisted.");
        await projectionService.SyncAsync(subscription, cancellationToken).ConfigureAwait(false);
    }

    private async Task SaveVenueAsync(VenueSubscription subscription, CancellationToken cancellationToken)
    {
        if (!await venueSubscriptionRepository.SaveAsync(subscription, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("The venue subscription could not be persisted.");
        featureResolutionService.Invalidate(subscription.VenueId);
    }

    private async Task RecordTierEventAsync(
        string eventId,
        Guid venueId,
        bool isNew,
        Guid? previousTierId,
        SubscriptionTier tier,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (isNew)
        {
            await RecordAsync(eventId, venueId, "signup", $"New {tier.Name} subscription", utcNow, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (previousTierId == tier.Id) return;
        var previousTier = previousTierId is null
            ? null
            : await billingCatalogRepository.GetByIdAsync(previousTierId.Value, cancellationToken).ConfigureAwait(false);
        var eventType = previousTier is not null && tier.Price < previousTier.Price ? "downgrade" : "upgrade";
        await RecordAsync(eventId, venueId, eventType, $"{(eventType == "upgrade" ? "Upgraded" : "Downgraded")} to {tier.Name}", utcNow, cancellationToken)
            .ConfigureAwait(false);
    }

    private Task RecordAsync(string sourceEventId, Guid venueId, string eventType, string summary, DateTime occurredUtc, CancellationToken cancellationToken) =>
        operationalEventRepository.AddAsync(new OperationalEvent
        {
            Id = CreateOperationalEventId(sourceEventId),
            VenueId = venueId,
            EventType = eventType,
            Summary = summary,
            OccurredUtc = occurredUtc
        }, cancellationToken);

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
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported subscription status.");
        return normalized;
    }

    private static string Normalize(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

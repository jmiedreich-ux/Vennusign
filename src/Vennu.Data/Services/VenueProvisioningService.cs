using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class VenueProvisioningService : IVenueProvisioningService
{
    private const string StarterTierSlug = "starter";

    private readonly IVenueRepository venueRepository;
    private readonly ISubscriptionTierRepository tierRepository;
    private readonly ISubscriptionManagementService subscriptionManagementService;
    private readonly IOrganizationSubscriptionRepository? organizationSubscriptions;
    private readonly IVenueEntitlementService? entitlementService;
    private readonly IOrganizationSubscriptionProjectionService? projectionService;

    public VenueProvisioningService(
        IVenueRepository venueRepository,
        ISubscriptionTierRepository tierRepository,
        ISubscriptionManagementService subscriptionManagementService,
        IOrganizationSubscriptionRepository? organizationSubscriptions = null,
        IVenueEntitlementService? entitlementService = null,
        IOrganizationSubscriptionProjectionService? projectionService = null)
    {
        this.venueRepository = venueRepository;
        this.tierRepository = tierRepository;
        this.subscriptionManagementService = subscriptionManagementService;
        this.organizationSubscriptions = organizationSubscriptions;
        this.entitlementService = entitlementService;
        this.projectionService = projectionService;
    }

    public async Task<VenueProvisioningResult> ProvisionAsync(
        Venue venue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(venue);
        NormalizeAndValidate(venue);

        if (venue.OrganizationId is Guid organizationId &&
            organizationSubscriptions is not null &&
            entitlementService is not null &&
            projectionService is not null)
        {
            await entitlementService.EnsureCanAddVenueAsync(organizationId, cancellationToken).ConfigureAwait(false);
            var organizationSubscription = await organizationSubscriptions
                .GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("An authoritative organization subscription is required.");
            var organizationVenueId = await venueRepository.CreateAsync(venue, cancellationToken).ConfigureAwait(false);
            var projection = await projectionService
                .SyncVenueAsync(organizationVenueId, organizationSubscription, cancellationToken).ConfigureAwait(false);
            return new VenueProvisioningResult(organizationVenueId, projection);
        }

        var starterTier = await tierRepository.GetBySlugAsync(StarterTierSlug, cancellationToken).ConfigureAwait(false);
        if (starterTier is null || !starterTier.IsActive)
            throw new InvalidOperationException("The Starter subscription tier is unavailable.");
        var venueId = await venueRepository
            .CreateAsync(venue, cancellationToken)
            .ConfigureAwait(false);
        var subscription = await subscriptionManagementService
            .StartTrialAsync(venueId, starterTier.Id, cancellationToken)
            .ConfigureAwait(false);

        return new VenueProvisioningResult(venueId, subscription);
    }

    private static void NormalizeAndValidate(Venue venue)
    {
        venue.Name = Required(venue.Name, 200, nameof(venue.Name));
        venue.Timezone = Required(venue.Timezone, 100, nameof(venue.Timezone));
        venue.Type = Required(venue.Type, 50, nameof(venue.Type));
        venue.PrimaryLanguage = Required(venue.PrimaryLanguage, 10, nameof(venue.PrimaryLanguage));
        venue.SecondaryLanguage = Optional(venue.SecondaryLanguage, 10, nameof(venue.SecondaryLanguage));
    }

    private static string Required(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"The value cannot exceed {maximumLength} characters.", parameterName);
        }

        return normalized;
    }

    private static string? Optional(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"The value cannot exceed {maximumLength} characters.", parameterName);
        }

        return normalized;
    }
}

using Vennu.Data.Repositories;
using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed class CheckoutSessionService(
    IBillingCatalogRepository billingCatalogRepository,
    IVenueSubscriptionRepository subscriptionRepository,
    IStripeCheckoutSessionGateway gateway,
    IVenueRepository? venueRepository = null,
    IOrganizationSubscriptionRepository? organizationSubscriptionRepository = null) : ICheckoutSessionService
{
    public async Task<StripeCheckoutSessionResult> CreateAsync(
        Guid venueId,
        Guid targetTierId,
        CheckoutBillingInterval billingInterval,
        CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        if (targetTierId == Guid.Empty)
        {
            throw new ArgumentException("Target tier ID is required.", nameof(targetTierId));
        }

        if (!Enum.IsDefined(billingInterval))
        {
            throw new ArgumentException("Billing interval must be monthly or annual.", nameof(billingInterval));
        }

        var venue = venueRepository is null
            ? null
            : await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"Venue '{venueId}' was not found.");
        var organizationSubscription = venue?.OrganizationId is Guid organizationId && organizationSubscriptionRepository is not null
            ? await organizationSubscriptionRepository.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            : null;
        if (organizationSubscription is null)
        {
            _ = await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"Venue '{venueId}' does not have a subscription.");
        }
        var tier = await billingCatalogRepository.GetByIdAsync(targetTierId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Subscription tier '{targetTierId}' does not exist.");

        if (!tier.IsActive || !tier.IsPublic)
        {
            throw new InvalidOperationException("The target tier is not available for self-service checkout.");
        }

        var priceId = billingInterval == CheckoutBillingInterval.Monthly
            ? tier.StripeMonthlyPriceId
            : tier.StripeAnnualPriceId;
        if (string.IsNullOrWhiteSpace(priceId))
        {
            throw new InvalidOperationException(
                $"The target tier does not have an active {billingInterval.ToString().ToLowerInvariant()} price mapping.");
        }

        return await gateway.CreateAsync(
            new StripeCheckoutSessionRequest(
                venueId,
                priceId,
                tier.Slug,
                venue?.OrganizationId,
                organizationSubscription?.StripeCustomerId),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<StripeCheckoutSessionResult> CreateForOrganizationAsync(
        Guid organizationId,
        Guid targetTierId,
        CheckoutBillingInterval billingInterval,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("Organization ID is required.", nameof(organizationId));
        if (organizationSubscriptionRepository is null)
            throw new InvalidOperationException("Organization billing is unavailable.");
        var subscription = await organizationSubscriptionRepository
            .GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false);
        var tier = await RequireTierAsync(targetTierId, billingInterval, cancellationToken).ConfigureAwait(false);
        var priceId = billingInterval == CheckoutBillingInterval.Monthly
            ? tier.StripeMonthlyPriceId!
            : tier.StripeAnnualPriceId!;
        return await gateway.CreateAsync(
            new StripeCheckoutSessionRequest(Guid.Empty, priceId, tier.Slug, organizationId, subscription?.StripeCustomerId),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<SubscriptionTier> RequireTierAsync(
        Guid targetTierId,
        CheckoutBillingInterval billingInterval,
        CancellationToken cancellationToken)
    {
        if (targetTierId == Guid.Empty) throw new ArgumentException("Target tier ID is required.", nameof(targetTierId));
        if (!Enum.IsDefined(billingInterval)) throw new ArgumentException("Billing interval must be monthly or annual.", nameof(billingInterval));
        var tier = await billingCatalogRepository.GetByIdAsync(targetTierId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Subscription tier '{targetTierId}' does not exist.");
        if (!tier.IsActive || !tier.IsPublic)
            throw new InvalidOperationException("The target tier is not available for self-service checkout.");
        var priceId = billingInterval == CheckoutBillingInterval.Monthly ? tier.StripeMonthlyPriceId : tier.StripeAnnualPriceId;
        if (string.IsNullOrWhiteSpace(priceId))
            throw new InvalidOperationException($"The target tier does not have an active {billingInterval.ToString().ToLowerInvariant()} price mapping.");
        return tier;
    }
}

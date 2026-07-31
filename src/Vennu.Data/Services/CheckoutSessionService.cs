using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class CheckoutSessionService(
    IBillingCatalogRepository billingCatalogRepository,
    IVenueSubscriptionRepository subscriptionRepository,
    IStripeCheckoutSessionGateway gateway) : ICheckoutSessionService
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

        _ = await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Venue '{venueId}' does not have a subscription.");
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
            new StripeCheckoutSessionRequest(venueId, priceId, tier.Slug),
            cancellationToken).ConfigureAwait(false);
    }
}

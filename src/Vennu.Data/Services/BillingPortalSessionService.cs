using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class BillingPortalSessionService(
    IVenueSubscriptionRepository subscriptionRepository,
    IStripeBillingPortalSessionGateway gateway) : IBillingPortalSessionService
{
    public async Task<StripeBillingPortalSessionResult> CreateAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        var subscription = await subscriptionRepository
            .GetByVenueIdAsync(venueId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Venue '{venueId}' does not have a subscription.");
        if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
        {
            throw new InvalidOperationException("Billing management is not available until the venue has a Stripe subscription.");
        }

        if (string.Equals(subscription.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Billing management is not available for a canceled subscription.");
        }

        return await gateway.CreateAsync(
            new StripeBillingPortalSessionRequest(subscription.StripeSubscriptionId.Trim()),
            cancellationToken).ConfigureAwait(false);
    }
}

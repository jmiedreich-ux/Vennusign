using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class BillingPortalSessionService(
    IVenueSubscriptionRepository subscriptionRepository,
    IStripeBillingPortalSessionGateway gateway,
    IVenueRepository? venueRepository = null,
    IOrganizationSubscriptionRepository? organizationSubscriptionRepository = null) : IBillingPortalSessionService
{
    public async Task<StripeBillingPortalSessionResult> CreateAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        var venue = venueRepository is null ? null : await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var organizationSubscription = venue?.OrganizationId is Guid organizationId && organizationSubscriptionRepository is not null
            ? await organizationSubscriptionRepository.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            : null;
        var legacySubscription = organizationSubscription is null
            ? await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            : null;
        var stripeSubscriptionId = organizationSubscription?.StripeSubscriptionId ?? legacySubscription?.StripeSubscriptionId;
        var status = organizationSubscription?.Status ?? legacySubscription?.Status
            ?? throw new KeyNotFoundException($"Venue '{venueId}' does not have a subscription.");
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
        {
            throw new InvalidOperationException("Billing management is not available until the organization has a Stripe subscription.");
        }

        if (string.Equals(status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Billing management is not available for a canceled subscription.");
        }

        return await gateway.CreateAsync(
            new StripeBillingPortalSessionRequest(stripeSubscriptionId.Trim()),
            cancellationToken).ConfigureAwait(false);
    }
}

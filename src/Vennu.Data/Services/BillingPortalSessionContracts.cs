namespace Vennu.Data.Services;

public sealed record StripeBillingPortalSessionRequest(string StripeSubscriptionId);

public sealed record StripeBillingPortalSessionResult(Uri PortalUrl);

public interface IStripeBillingPortalSessionGateway
{
    Task<StripeBillingPortalSessionResult> CreateAsync(
        StripeBillingPortalSessionRequest request,
        CancellationToken cancellationToken = default);
}

public interface IBillingPortalSessionService
{
    Task<StripeBillingPortalSessionResult> CreateAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);
}

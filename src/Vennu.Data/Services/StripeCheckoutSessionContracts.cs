namespace Vennu.Data.Services;

public enum CheckoutBillingInterval
{
    Monthly,
    Annual
}

public sealed record StripeCheckoutSessionRequest(
    Guid VenueId,
    string PriceId,
    string TierSlug);

public sealed record StripeCheckoutSessionResult(Uri CheckoutUrl);

public interface IStripeCheckoutSessionGateway
{
    Task<StripeCheckoutSessionResult> CreateAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);
}

public interface ICheckoutSessionService
{
    Task<StripeCheckoutSessionResult> CreateAsync(
        Guid venueId,
        Guid targetTierId,
        CheckoutBillingInterval billingInterval,
        CancellationToken cancellationToken = default);
}

namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record CreateCheckoutSessionRequest(
    Guid TargetTierId,
    string BillingInterval);

public sealed record CreateCheckoutSessionResponse(string CheckoutUrl);

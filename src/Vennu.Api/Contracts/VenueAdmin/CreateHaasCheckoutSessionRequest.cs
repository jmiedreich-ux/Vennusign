namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record CreateHaasCheckoutSessionRequest(
    string BundleKey,
    int TermMonths);

public sealed record CreateHaasCheckoutSessionResponse(string CheckoutUrl);

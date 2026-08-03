namespace Vennu.Api.Contracts.BackOffice;

public sealed record CreateHaasCheckoutSessionRequest(
    string BundleKey,
    int TermMonths);

public sealed record CreateHaasCheckoutSessionResponse(string CheckoutUrl);

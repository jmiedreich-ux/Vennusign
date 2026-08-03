namespace Vennu.Api.Contracts.BackOffice;

public sealed record CreateCheckoutSessionRequest(
    Guid TargetTierId,
    string BillingInterval);

public sealed record CreateCheckoutSessionResponse(string CheckoutUrl);

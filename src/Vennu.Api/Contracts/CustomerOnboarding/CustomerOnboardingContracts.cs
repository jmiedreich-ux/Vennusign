namespace Vennu.Api.Contracts.CustomerOnboarding;

public sealed record CreateOnboardingOrganizationRequest(string Name);
public sealed record SelectOnboardingTrialRequest(Guid TierId);
public sealed record CreateOnboardingCheckoutRequest(Guid TierId, string BillingInterval);
public sealed record CreateOnboardingCheckoutResponse(string CheckoutUrl);

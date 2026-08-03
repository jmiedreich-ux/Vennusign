namespace Vennu.Api.Contracts.CustomerOnboarding;

public sealed record CreateOnboardingOrganizationRequest(
    string Name,
    string? LegalName,
    string PrimaryContactName,
    string ContactEmail,
    string? ContactPhone,
    string MailingAddress);
public sealed record SelectOnboardingTrialRequest(Guid TierId);
public sealed record CreateOnboardingCheckoutRequest(Guid TierId, string BillingInterval);
public sealed record CreateOnboardingCheckoutResponse(string CheckoutUrl);
public sealed record CreateOnboardingVenueRequest(
    string Name,
    string Timezone,
    string Type,
    string PrimaryLanguage,
    string? SecondaryLanguage);
public sealed record ClaimOnboardingFirstScreenRequest(string Code);

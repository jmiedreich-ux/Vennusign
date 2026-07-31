namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record VenueAdminBillingPresentationResponse(
    VenueAdminTierSummary? CurrentTier,
    VenueAdminSubscriptionSummary? Subscription,
    IReadOnlyCollection<VenueAdminTierSummary> AvailableTiers,
    IReadOnlyDictionary<string, VenueAdminFeatureSummary> EffectiveFeatures,
    IReadOnlyCollection<VenueAdminHaasBundleSummary> HaasBundles,
    VenueAdminHaasContractSummary? HaasContract);

public sealed record VenueAdminTierSummary(
    Guid Id,
    string Name,
    string Slug,
    decimal MonthlyPrice,
    int MaxScreens);

public sealed record VenueAdminSubscriptionSummary(
    string Status,
    DateTime? TrialEndsAt,
    DateTime? CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    bool CanManageBilling);

public sealed record VenueAdminHaasBundleSummary(
    string Key,
    string Name,
    int TermMonths,
    decimal MonthlyAmount,
    string PostContractTierSlug);

public sealed record VenueAdminHaasContractSummary(
    string BundleKey,
    string BundleName,
    string Status,
    int TermMonths,
    decimal MonthlyAmount,
    DateTime StartedUtc,
    DateTime ContractEndsUtc,
    int RemainingMonths,
    decimal EstimatedBuyoutAmount,
    bool CancelAtPeriodEnd,
    DateTime? EndedUtc);

public sealed record VenueAdminFeatureSummary(
    bool Enabled,
    string? LimitValue);

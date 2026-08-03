namespace Vennu.Api.Contracts.BackOffice;

public sealed record BackOfficeBillingPresentationResponse(
    BackOfficeTierSummary? CurrentTier,
    BackOfficeSubscriptionSummary? Subscription,
    IReadOnlyCollection<BackOfficeTierSummary> AvailableTiers,
    IReadOnlyDictionary<string, BackOfficeFeatureSummary> EffectiveFeatures,
    IReadOnlyCollection<BackOfficeHaasBundleSummary> HaasBundles,
    BackOfficeHaasContractSummary? HaasContract);

public sealed record BackOfficeTierSummary(
    Guid Id,
    string Name,
    string Slug,
    decimal MonthlyPrice,
    int MaxScreens);

public sealed record BackOfficeSubscriptionSummary(
    string Status,
    DateTime? TrialEndsAt,
    DateTime? CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    bool CanManageBilling);

public sealed record BackOfficeHaasBundleSummary(
    string Key,
    string Name,
    int TermMonths,
    decimal MonthlyAmount,
    string PostContractTierSlug);

public sealed record BackOfficeHaasContractSummary(
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

public sealed record BackOfficeFeatureSummary(
    bool Enabled,
    string? LimitValue);

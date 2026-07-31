namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record VenueAdminBillingPresentationResponse(
    VenueAdminTierSummary? CurrentTier,
    IReadOnlyCollection<VenueAdminTierSummary> AvailableTiers,
    IReadOnlyDictionary<string, VenueAdminFeatureSummary> EffectiveFeatures);

public sealed record VenueAdminTierSummary(
    Guid Id,
    string Name,
    string Slug,
    decimal MonthlyPrice,
    int MaxScreens);

public sealed record VenueAdminFeatureSummary(
    bool Enabled,
    string? LimitValue);

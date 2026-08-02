namespace Vennu.Data.Services;

public sealed record TierManagementRequest(
    string Name,
    string Slug,
    decimal Price,
    int MaxScreens,
    bool IsPublic,
    bool IsActive,
    string? StripeProductId,
    string? StripeMonthlyPriceId,
    string? StripeAnnualPriceId,
    int MaxVenues = 1,
    int TrialDays = 14,
    string TrialExpiryBehavior = "disable");

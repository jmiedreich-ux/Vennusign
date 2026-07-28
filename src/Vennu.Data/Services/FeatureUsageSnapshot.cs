namespace Vennu.Data.Services;

public sealed record FeatureUsageSnapshot(
    string FeatureKey,
    DateTime PeriodStartUtc,
    int Used,
    int? Limit,
    int? Remaining);

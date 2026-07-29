namespace Vennu.Data.Services;

public sealed record RevenueTrend(
    string Currency,
    IReadOnlyCollection<RevenueTrendPoint> Points);

public sealed record RevenueTrendPoint(
    DateTime MonthUtc,
    decimal Mrr,
    int ActiveSubscriptions,
    decimal? MrrChangePercent);

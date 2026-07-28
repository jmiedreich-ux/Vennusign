namespace Vennu.Data.Services;

public sealed record RevenueSnapshot(
    string Currency,
    decimal Mrr,
    decimal Arr,
    decimal AverageRevenuePerActiveSubscription,
    int ActiveSubscriptions,
    IReadOnlyCollection<TierRevenue> Tiers,
    decimal UnmatchedMrr,
    IReadOnlyCollection<string> UnmatchedPriceIds);

public sealed record TierRevenue(Guid TierId, string TierName, decimal Mrr);

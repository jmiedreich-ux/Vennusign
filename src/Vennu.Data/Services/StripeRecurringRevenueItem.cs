namespace Vennu.Data.Services;

public sealed record StripeRecurringRevenueItem(
    string SubscriptionId,
    string PriceId,
    string Currency,
    decimal UnitAmountMinor,
    long Quantity,
    string Interval,
    long IntervalCount);

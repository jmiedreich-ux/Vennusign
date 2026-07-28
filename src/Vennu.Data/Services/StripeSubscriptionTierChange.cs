namespace Vennu.Data.Services;

public sealed record StripeSubscriptionTierChange(
    string SubscriptionItemId,
    string PreviousPriceId,
    string NewPriceId);

namespace Vennu.Data.Services;

public interface IStripeSubscriptionTierUpdater
{
    Task<StripeSubscriptionTierChange> ChangeAsync(
        string stripeSubscriptionId,
        string monthlyPriceId,
        string? annualPriceId,
        CancellationToken cancellationToken = default);

    Task RestoreAsync(
        StripeSubscriptionTierChange change,
        CancellationToken cancellationToken = default);
}

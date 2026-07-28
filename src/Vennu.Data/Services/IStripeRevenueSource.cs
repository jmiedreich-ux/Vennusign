namespace Vennu.Data.Services;

public interface IStripeRevenueSource
{
    Task<IReadOnlyCollection<StripeRecurringRevenueItem>> GetActiveItemsAsync(
        CancellationToken cancellationToken = default);
}

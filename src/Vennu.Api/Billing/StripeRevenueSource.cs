using Microsoft.Extensions.Options;
using Stripe;
using Vennu.Data.Services;

namespace Vennu.Api.Billing;

public sealed class StripeRevenueSource(IOptions<StripeRevenueOptions> options) : IStripeRevenueSource
{
    public async Task<IReadOnlyCollection<StripeRecurringRevenueItem>> GetActiveItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var apiKey = options.Value.ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey) ||
            !(apiKey.StartsWith("sk_", StringComparison.Ordinal) ||
              apiKey.StartsWith("rk_", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Stripe revenue API access is not configured.");
        }

        var service = new SubscriptionService(new StripeClient(apiKey));
        var result = new List<StripeRecurringRevenueItem>();
        var query = new SubscriptionListOptions
        {
            Status = "active",
            Limit = 100
        };

        await foreach (var subscription in service.ListAutoPagingAsync(
            query,
            cancellationToken: cancellationToken))
        {
            foreach (var item in subscription.Items?.Data ?? [])
            {
                var price = item.Price;
                if (price?.Recurring is null ||
                    price.UnitAmountDecimal is null ||
                    !string.Equals(price.BillingScheme, "per_unit", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Active Stripe subscription '{subscription.Id}' contains an unsupported price.");
                }

                result.Add(new StripeRecurringRevenueItem(
                    subscription.Id,
                    price.Id,
                    price.Currency,
                    price.UnitAmountDecimal.Value,
                    item.Quantity,
                    price.Recurring.Interval,
                    price.Recurring.IntervalCount));
            }
        }

        return result;
    }
}

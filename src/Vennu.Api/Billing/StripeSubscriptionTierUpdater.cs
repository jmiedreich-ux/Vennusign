using Microsoft.Extensions.Options;
using Stripe;
using Vennu.Data.Services;

namespace Vennu.Api.Billing;

public sealed class StripeSubscriptionTierUpdater(IOptions<StripeRevenueOptions> options)
    : IStripeSubscriptionTierUpdater
{
    public async Task<StripeSubscriptionTierChange> ChangeAsync(
        string stripeSubscriptionId,
        string monthlyPriceId,
        string? annualPriceId,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        var subscription = await new SubscriptionService(client)
            .GetAsync(stripeSubscriptionId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var item = subscription.Items?.Data?.SingleOrDefault()
            ?? throw new InvalidOperationException("Stripe tier switching requires exactly one subscription item.");
        var previousPrice = item.Price
            ?? throw new InvalidOperationException("The Stripe subscription item does not expose its current price.");
        var targetPriceId = string.Equals(previousPrice.Recurring?.Interval, "year", StringComparison.OrdinalIgnoreCase)
            ? annualPriceId?.Trim()
            : monthlyPriceId.Trim();
        if (string.IsNullOrWhiteSpace(targetPriceId))
        {
            throw new InvalidOperationException("The target tier does not have a Stripe price for the current billing interval.");
        }

        await new SubscriptionItemService(client)
            .UpdateAsync(
                item.Id,
                new SubscriptionItemUpdateOptions
                {
                    Price = targetPriceId,
                    ProrationBehavior = "create_prorations"
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new StripeSubscriptionTierChange(item.Id, previousPrice.Id, targetPriceId);
    }

    public async Task RestoreAsync(
        StripeSubscriptionTierChange change,
        CancellationToken cancellationToken = default)
    {
        await new SubscriptionItemService(CreateClient())
            .UpdateAsync(
                change.SubscriptionItemId,
                new SubscriptionItemUpdateOptions
                {
                    Price = change.PreviousPriceId,
                    ProrationBehavior = "create_prorations"
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private StripeClient CreateClient()
    {
        var apiKey = options.Value.ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey) ||
            !(apiKey.StartsWith("sk_", StringComparison.Ordinal) ||
              apiKey.StartsWith("rk_", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Stripe subscription management API access is not configured.");
        }

        return new StripeClient(apiKey);
    }
}

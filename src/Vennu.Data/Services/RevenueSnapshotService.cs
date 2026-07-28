using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class RevenueSnapshotService(
    IStripeRevenueSource source,
    ISubscriptionTierRepository tierRepository) : IRevenueSnapshotService
{
    public async Task<RevenueSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var itemsTask = source.GetActiveItemsAsync(cancellationToken);
        var tiersTask = tierRepository.GetAllAsync(cancellationToken);
        await Task.WhenAll(itemsTask, tiersTask).ConfigureAwait(false);

        var items = itemsTask.Result;
        var tiers = tiersTask.Result;
        var currencies = items
            .Select(item => item.Currency)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (currencies.Length > 1 ||
            (currencies.Length == 1 && !string.Equals(currencies[0], "usd", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("The revenue dashboard requires all active Stripe prices to use USD.");
        }

        var priceToTier = BuildPriceMap(tiers);
        var tierMrr = tiers.ToDictionary(tier => tier.Id, _ => 0m);
        var unmatchedPriceIds = new HashSet<string>(StringComparer.Ordinal);
        var unmatchedMrr = 0m;
        var totalMrr = 0m;

        foreach (var item in items)
        {
            var itemMrr = MonthlyMajorAmount(item);
            totalMrr += itemMrr;
            if (priceToTier.TryGetValue(item.PriceId, out var tier))
            {
                tierMrr[tier.Id] += itemMrr;
            }
            else
            {
                unmatchedMrr += itemMrr;
                unmatchedPriceIds.Add(item.PriceId);
            }
        }

        var activeSubscriptions = items
            .Select(item => item.SubscriptionId)
            .Distinct(StringComparer.Ordinal)
            .Count();
        totalMrr = decimal.Round(totalMrr, 2, MidpointRounding.AwayFromZero);

        return new RevenueSnapshot(
            "USD",
            totalMrr,
            decimal.Round(totalMrr * 12, 2, MidpointRounding.AwayFromZero),
            activeSubscriptions == 0
                ? 0
                : decimal.Round(totalMrr / activeSubscriptions, 2, MidpointRounding.AwayFromZero),
            activeSubscriptions,
            tiers
                .OrderBy(tier => tier.Name, StringComparer.OrdinalIgnoreCase)
                .Select(tier => new TierRevenue(
                    tier.Id,
                    tier.Name,
                    decimal.Round(tierMrr[tier.Id], 2, MidpointRounding.AwayFromZero)))
                .ToArray(),
            decimal.Round(unmatchedMrr, 2, MidpointRounding.AwayFromZero),
            unmatchedPriceIds.Order(StringComparer.Ordinal).ToArray());
    }

    private static IReadOnlyDictionary<string, SubscriptionTier> BuildPriceMap(
        IReadOnlyCollection<SubscriptionTier> tiers)
    {
        var result = new Dictionary<string, SubscriptionTier>(StringComparer.Ordinal);
        foreach (var tier in tiers)
        {
            Add(tier.StripeMonthlyPriceId, tier);
            Add(tier.StripeAnnualPriceId, tier);
        }

        return result;

        void Add(string? priceId, SubscriptionTier tier)
        {
            if (string.IsNullOrWhiteSpace(priceId))
            {
                return;
            }

            if (!result.TryAdd(priceId, tier))
            {
                throw new InvalidOperationException($"Stripe price '{priceId}' is mapped to more than one tier.");
            }
        }
    }

    private static decimal MonthlyMajorAmount(StripeRecurringRevenueItem item)
    {
        if (item.UnitAmountMinor < 0 || item.Quantity < 1 || item.IntervalCount < 1)
        {
            throw new InvalidOperationException($"Stripe price '{item.PriceId}' has invalid recurring amount metadata.");
        }

        var months = item.Interval.ToLowerInvariant() switch
        {
            "month" => item.IntervalCount,
            "year" => item.IntervalCount * 12,
            _ => throw new InvalidOperationException(
                $"Stripe price '{item.PriceId}' must use a monthly or annual recurring interval.")
        };

        return item.UnitAmountMinor * item.Quantity / months / 100m;
    }
}

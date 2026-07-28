using Vennu.Api.Billing;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Services;
using Microsoft.Extensions.Options;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class RevenueSnapshotServiceTests
{
    [Fact]
    public async Task GetAsync_NormalizesMonthlyAndAnnualPricesAndMapsTiers()
    {
        var proId = Guid.NewGuid();
        var service = new RevenueSnapshotService(
            new FakeStripeRevenueSource(
            [
                new("sub_monthly", "price_pro_month", "usd", 8900m, 2, "month", 1),
                new("sub_annual", "price_pro_year", "USD", 96000m, 1, "year", 1),
                new("sub_unmatched", "price_legacy", "usd", 2500m, 1, "month", 1)
            ]),
            new FakeSubscriptionTierRepository
            {
                Items =
                [
                    new SubscriptionTier
                    {
                        Id = proId,
                        Name = "Pro",
                        StripeMonthlyPriceId = "price_pro_month",
                        StripeAnnualPriceId = "price_pro_year"
                    },
                    new SubscriptionTier { Id = Guid.NewGuid(), Name = "Starter" }
                ]
            });

        var result = await service.GetAsync();

        Assert.Equal("USD", result.Currency);
        Assert.Equal(283m, result.Mrr);
        Assert.Equal(3396m, result.Arr);
        Assert.Equal(94.33m, result.AverageRevenuePerActiveSubscription);
        Assert.Equal(3, result.ActiveSubscriptions);
        Assert.Equal(258m, Assert.Single(result.Tiers, tier => tier.TierId == proId).Mrr);
        Assert.Equal(0m, Assert.Single(result.Tiers, tier => tier.TierName == "Starter").Mrr);
        Assert.Equal(25m, result.UnmatchedMrr);
        Assert.Equal(new[] { "price_legacy" }, result.UnmatchedPriceIds);
    }

    [Fact]
    public async Task GetAsync_CountsMultiItemSubscriptionOnce()
    {
        var service = new RevenueSnapshotService(
            new FakeStripeRevenueSource(
            [
                new("sub_one", "price_a", "usd", 1000m, 1, "month", 1),
                new("sub_one", "price_b", "usd", 500m, 1, "month", 1)
            ]),
            new FakeSubscriptionTierRepository());

        var result = await service.GetAsync();

        Assert.Equal(1, result.ActiveSubscriptions);
        Assert.Equal(15m, result.AverageRevenuePerActiveSubscription);
    }

    [Fact]
    public async Task GetAsync_RejectsUnsupportedCurrency()
    {
        var service = new RevenueSnapshotService(
            new FakeStripeRevenueSource(
            [
                new("sub_one", "price_eur", "eur", 1000m, 1, "month", 1)
            ]),
            new FakeSubscriptionTierRepository());

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAsync());

        Assert.Contains("USD", error.Message);
    }

    [Fact]
    public async Task StripeSource_RejectsMissingApiKeyBeforeNetworkAccess()
    {
        var source = new StripeRevenueSource(Options.Create(new StripeRevenueOptions()));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => source.GetActiveItemsAsync());

        Assert.Equal("Stripe revenue API access is not configured.", error.Message);
    }

    private sealed class FakeStripeRevenueSource(
        IReadOnlyCollection<StripeRecurringRevenueItem> items) : IStripeRevenueSource
    {
        public Task<IReadOnlyCollection<StripeRecurringRevenueItem>> GetActiveItemsAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(items);
    }
}

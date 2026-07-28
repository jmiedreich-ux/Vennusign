using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public class BillingCatalogServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 28, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Public_catalog_returns_active_public_tiers_in_price_order()
    {
        var repository = new BillingCatalogRepositoryFake(
            Tier("pro", 89m),
            Tier("starter", 39m),
            Tier("private", 10m, isPublic: false),
            Tier("retired", 20m, isActive: false));
        var service = CreateService(repository);

        var catalog = await service.GetPublicCatalogAsync();

        Assert.Equal(new[] { "starter", "pro" }, catalog.Select(item => item.Slug));
        Assert.Equal(390m, catalog.First().AnnualPrice);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Configure_stripe_persists_complete_catalog_metadata()
    {
        var tier = Tier("starter", 39m);
        var repository = new BillingCatalogRepositoryFake(tier);
        var service = CreateService(repository);

        var result = await service.ConfigureStripeAsync(tier.Id, "prod_starter", "price_starter_month", "price_starter_year");

        Assert.True(result.IsStripeConfigured);
        Assert.Equal("prod_starter", tier.StripeProductId);
        Assert.Equal("price_starter_month", tier.StripeMonthlyPriceId);
        Assert.Equal("price_starter_year", tier.StripeAnnualPriceId);
        Assert.Equal(UtcNow.UtcDateTime, tier.UpdatedUtc);
        Assert.Same(tier, Assert.Single(repository.Saved));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Configure_stripe_rejects_same_monthly_and_annual_price()
    {
        var tier = Tier("starter", 39m);
        var service = CreateService(new BillingCatalogRepositoryFake(tier));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ConfigureStripeAsync(tier.Id, "prod_starter", "price_same", "price_same"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Configure_stripe_rejects_identifier_owned_by_another_tier()
    {
        var starter = Tier("starter", 39m);
        var pro = Tier("pro", 89m);
        pro.StripeMonthlyPriceId = "price_existing";
        var service = CreateService(new BillingCatalogRepositoryFake(starter, pro));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConfigureStripeAsync(starter.Id, "prod_starter", "price_existing", "price_starter_year"));
    }

    private static BillingCatalogService CreateService(BillingCatalogRepositoryFake repository) =>
        new(repository, new FixedTimeProvider(UtcNow));

    private static SubscriptionTier Tier(
        string slug,
        decimal price,
        bool isPublic = true,
        bool isActive = true) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = slug,
            Slug = slug,
            Price = price,
            MaxScreens = 1,
            IsPublic = isPublic,
            IsActive = isActive
        };

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class BillingCatalogRepositoryFake(params SubscriptionTier[] tiers) : IBillingCatalogRepository
    {
        private readonly List<SubscriptionTier> tiers = tiers.ToList();
        public List<SubscriptionTier> Saved { get; } = [];

        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<SubscriptionTier>>(tiers);

        public Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default) =>
            Task.FromResult(tiers.SingleOrDefault(tier => tier.Id == tierId));

        public Task<SubscriptionTier?> GetByStripeProductIdAsync(string productId, CancellationToken cancellationToken = default) =>
            Task.FromResult(tiers.SingleOrDefault(tier => tier.StripeProductId == productId));

        public Task<SubscriptionTier?> GetByStripePriceIdAsync(string priceId, CancellationToken cancellationToken = default) =>
            Task.FromResult(tiers.SingleOrDefault(tier =>
                tier.StripeMonthlyPriceId == priceId ||
                tier.StripeAnnualPriceId == priceId));

        public Task<bool> SaveAsync(SubscriptionTier tier, CancellationToken cancellationToken = default)
        {
            Saved.Add(tier);
            return Task.FromResult(true);
        }
    }
}

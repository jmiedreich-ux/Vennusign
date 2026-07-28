using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class TierManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesAndPersistsTier()
    {
        var repository = new FakeSubscriptionTierRepository();
        var service = new TierManagementService(repository, TimeProvider.System);

        var tier = await service.CreateAsync(Request("  Pro Plan  ", " Pro Plan "));

        Assert.Equal("Pro Plan", tier.Name);
        Assert.Equal("pro-plan", tier.Slug);
        Assert.Same(tier, Assert.Single(repository.Items));
    }

    [Theory]
    [InlineData(-0.01, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -2)]
    public async Task CreateAsync_RejectsInvalidCommercialLimits(decimal price, int maxScreens)
    {
        var service = new TierManagementService(new FakeSubscriptionTierRepository(), TimeProvider.System);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateAsync(Request("Pro", "pro", price, maxScreens)));
    }

    [Fact]
    public async Task CloneAsync_CreatesPrivateInactiveCopyWithoutStripeMappings()
    {
        var source = new SubscriptionTier
        {
            Id = Guid.NewGuid(), Name = "Pro", Slug = "pro", Price = 49, MaxScreens = 8,
            IsPublic = true, IsActive = true, StripeProductId = "prod_1",
            StripeMonthlyPriceId = "price_month", StripeAnnualPriceId = "price_year"
        };
        var repository = new FakeSubscriptionTierRepository { Items = [source] };
        var service = new TierManagementService(repository, TimeProvider.System);

        var clone = await service.CloneAsync(source.Id);

        Assert.NotNull(clone);
        Assert.Equal("pro-copy", clone.Slug);
        Assert.False(clone.IsPublic);
        Assert.False(clone.IsActive);
        Assert.Null(clone.StripeProductId);
        Assert.Null(clone.StripeMonthlyPriceId);
        Assert.Null(clone.StripeAnnualPriceId);
    }

    [Fact]
    public async Task ArchiveAsync_DeactivatesWithoutDeletingTier()
    {
        var tier = new SubscriptionTier { Id = Guid.NewGuid(), Name = "Pro", Slug = "pro", IsActive = true };
        var repository = new FakeSubscriptionTierRepository { Items = [tier] };
        var service = new TierManagementService(repository, TimeProvider.System);

        Assert.True(await service.ArchiveAsync(tier.Id));
        Assert.False(Assert.Single(repository.Items).IsActive);
    }

    private static TierManagementRequest Request(
        string name,
        string slug,
        decimal price = 49,
        int maxScreens = 8) =>
        new(name, slug, price, maxScreens, true, true, "prod_1", "price_month", "price_year");
}

using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public class FeatureTierRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task FeatureRepository_GetByKeyAsync_UsesFeatureKey()
    {
        var expected = new Feature { Id = Guid.NewGuid(), Key = "happy_hour", Label = "Happy Hour" };
        object? criteria = null;
        var dataAccess = new FakeSqlDataAccess
        {
            QueryHandler = value =>
            {
                criteria = value;
                return expected;
            }
        };

        var result = await new FeatureRepository(dataAccess).GetByKeyAsync("happy_hour");

        Assert.Same(expected, result);
        Assert.Equal("happy_hour", criteria?.GetType().GetProperty("Key")?.GetValue(criteria));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubscriptionTierRepository_GetFeaturesAsync_ReturnsTierMappings()
    {
        var tierId = Guid.NewGuid();
        var mappings = new[] { new TierFeature { TierId = tierId, FeatureId = Guid.NewGuid(), LimitValue = "1" } };
        var dataAccess = new FakeSqlDataAccess { QueryManyHandler = _ => mappings };

        var result = await new SubscriptionTierRepository(dataAccess).GetFeaturesAsync(tierId);

        Assert.Single(result);
        Assert.Equal("1", result.Single().LimitValue);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task VenueSubscriptionRepository_SaveAsync_InsertsNewSubscription()
    {
        var subscription = new VenueSubscription
        {
            VenueId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Status = "trialing"
        };
        var dataAccess = new FakeSqlDataAccess { QueryHandler = _ => null, InsertResult = 1 };

        var saved = await new VenueSubscriptionRepository(dataAccess).SaveAsync(subscription);

        Assert.True(saved);
        Assert.Single(dataAccess.InsertedEntities);
        Assert.Empty(dataAccess.UpdatedEntities);
        Assert.NotEqual(default, subscription.CreatedUtc);
        Assert.NotEqual(default, subscription.UpdatedUtc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task VenueSubscriptionRepository_SaveAsync_UpdatesExistingSubscription()
    {
        var subscription = new VenueSubscription
        {
            VenueId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Status = "active",
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        };
        var dataAccess = new FakeSqlDataAccess { QueryHandler = _ => subscription, UpdateResult = 1 };

        var saved = await new VenueSubscriptionRepository(dataAccess).SaveAsync(subscription);

        Assert.True(saved);
        Assert.Empty(dataAccess.InsertedEntities);
        Assert.Single(dataAccess.UpdatedEntities);
    }
}

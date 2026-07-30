using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public sealed class MealPeriodRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateAsync_AssignsIdentityAndTimestamps()
    {
        var dataAccess = new FakeSqlDataAccess();
        var repository = new MealPeriodRepository(dataAccess);
        var mealPeriod = new MealPeriod
        {
            VenueId = Guid.NewGuid(),
            Name = "Breakfast",
            StartLocalTime = TimeSpan.FromHours(7),
            EndLocalTime = TimeSpan.FromHours(11),
            SortOrder = 0
        };

        var id = await repository.CreateAsync(mealPeriod);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(id, mealPeriod.Id);
        Assert.NotEqual(default, mealPeriod.CreatedUtc);
        Assert.NotEqual(default, mealPeriod.UpdatedUtc);
        Assert.Same(mealPeriod, Assert.Single(dataAccess.InsertedEntities));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetByVenueIdAsync_UsesVenueScopeAndDeterministicOrder()
    {
        string? capturedSql = null;
        object? capturedParameters = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, parameters) =>
            {
                capturedSql = sql;
                capturedParameters = parameters;
                return [];
            }
        };
        var repository = new MealPeriodRepository(dataAccess);
        var venueId = Guid.NewGuid();

        await repository.GetByVenueIdAsync(venueId);

        Assert.Contains("WHERE VenueId = @VenueId", capturedSql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY SortOrder, Id", capturedSql, StringComparison.Ordinal);
        Assert.Equal(venueId, capturedParameters!.GetType().GetProperty("VenueId")!.GetValue(capturedParameters));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetByVenueIdAsync_RejectsEmptyVenueId()
    {
        var repository = new MealPeriodRepository(new FakeSqlDataAccess());

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => repository.GetByVenueIdAsync(Guid.Empty));

        Assert.Equal("venueId", exception.ParamName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteAsync_UsesVenueAndPeriodScope()
    {
        string? capturedSql = null;
        object? capturedParameters = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, parameters) =>
            {
                capturedSql = sql;
                capturedParameters = parameters;
                return [new MealPeriodRepository.RemovalResult { Removed = true }];
            }
        };
        var repository = new MealPeriodRepository(dataAccess);
        var venueId = Guid.NewGuid();
        var periodId = Guid.NewGuid();

        Assert.True(await repository.DeleteAsync(venueId, periodId));
        Assert.Contains("VenueId = @VenueId AND Id = @MealPeriodId", capturedSql, StringComparison.Ordinal);
        Assert.Equal(venueId, capturedParameters!.GetType().GetProperty("VenueId")!.GetValue(capturedParameters));
        Assert.Equal(periodId, capturedParameters.GetType().GetProperty("MealPeriodId")!.GetValue(capturedParameters));
    }
}

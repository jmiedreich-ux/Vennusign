using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public sealed class MenuRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateItemAsync_AssignsIdentityAndTimestamps()
    {
        var dataAccess = new FakeSqlDataAccess();
        var repository = new MenuRepository(dataAccess);
        var item = new MenuItem
        {
            VenueId = Guid.NewGuid(),
            MenuSectionId = Guid.NewGuid(),
            Name = "Burger",
            Price = 12.95m
        };

        var id = await repository.CreateItemAsync(item);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(id, item.Id);
        Assert.NotEqual(default, item.CreatedUtc);
        Assert.NotEqual(default, item.UpdatedUtc);
        Assert.Same(item, Assert.Single(dataAccess.InsertedEntities));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetSectionsAsync_UsesVenueScopeAndDeterministicOrder()
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
        var repository = new MenuRepository(dataAccess);
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();

        await repository.GetSectionsAsync(venueId, menuId);

        Assert.Contains("VenueId = @VenueId AND MenuId = @MenuId", capturedSql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY SortOrder, Id", capturedSql, StringComparison.Ordinal);
        Assert.Equal(venueId, capturedParameters!.GetType().GetProperty("VenueId")!.GetValue(capturedParameters));
        Assert.Equal(menuId, capturedParameters.GetType().GetProperty("MenuId")!.GetValue(capturedParameters));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetItemsAsync_UsesVenueScopeAndStableTieBreaker()
    {
        string? capturedSql = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, _) =>
            {
                capturedSql = sql;
                return [];
            }
        };
        var repository = new MenuRepository(dataAccess);

        await repository.GetItemsAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Contains("VenueId = @VenueId AND MenuSectionId = @MenuSectionId", capturedSql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY SortOrder, Id", capturedSql, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetTranslationsAsync_RejectsEmptyVenueId()
    {
        var repository = new MenuRepository(new FakeSqlDataAccess());

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => repository.GetTranslationsAsync(Guid.Empty, Guid.NewGuid()));

        Assert.Equal("venueId", exception.ParamName);
    }
}

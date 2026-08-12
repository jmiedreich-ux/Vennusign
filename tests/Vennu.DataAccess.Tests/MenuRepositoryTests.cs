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

    // Security regression: the screen id arrives from the route, so the assignment
    // must be scoped to the calling venue on both sides. Without this, one venue
    // could pass another venue's screen id and take over its assignment.
    [Fact]
    [Trait("Category", "Unit")]
    public async Task AssignScreenAsync_ScopesBothScreenAndMenuToTheCallingVenue()
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
        var repository = new ContentRepository(dataAccess);

        // No row comes back because the venue does not own the screen.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.AssignScreenAsync(new MenuScreenAssignment
            {
                VenueId = Guid.NewGuid(),
                ScreenId = Guid.NewGuid(),
                MenuId = Guid.NewGuid(),
                PageId = Guid.NewGuid()
            }));

        Assert.Contains("s.VenueId = @VenueId", capturedSql, StringComparison.Ordinal);
        Assert.Contains("m.VenueId = @VenueId", capturedSql, StringComparison.Ordinal);
    }

    // Regression: the menu read must select the milestone-1 settings columns.
    // While they were missing from the SELECT, stored values were silently
    // replaced by the model's C# defaults, so a published menu still reported
    // PublishedVersion null.
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetMenusAsync_SelectsTheMenuSettingsAndPublishedVersion()
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

        await repository.GetMenusAsync(Guid.NewGuid());

        foreach (var column in new[] { "DwellSeconds", "LoopWarningSeconds", "Theme", "IsPutAway", "PublishedVersion" })
        {
            Assert.Contains(column, capturedSql, StringComparison.Ordinal);
        }
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

}

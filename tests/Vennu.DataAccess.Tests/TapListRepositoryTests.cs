using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class TapListRepositoryTests
{
    [Fact]
    public async Task CreateItemAsync_AssignsIdentityAndTimestamps()
    {
        var dataAccess = new FakeSqlDataAccess();
        var repository = new TapListRepository(dataAccess);
        var item = new TapItem { VenueId = Guid.NewGuid(), Name = "480B", Price = 7m };

        var id = await repository.CreateItemAsync(item);

        Assert.Equal(id, item.Id);
        Assert.NotEqual(Guid.Empty, id);
        Assert.NotEqual(default, item.CreatedUtc);
        Assert.NotEqual(default, item.UpdatedUtc);
        Assert.Same(item, Assert.Single(dataAccess.InsertedEntities));
    }

    [Fact]
    public async Task GetItemsAsync_UsesVenueScopeAndStableOrder()
    {
        string? sql = null;
        object? parameters = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (value, args) => { sql = value; parameters = args; return []; }
        };
        var repository = new TapListRepository(dataAccess);
        var venueId = Guid.NewGuid();

        await repository.GetItemsAsync(venueId);

        Assert.Contains("WHERE VenueId = @VenueId", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY SortOrder, Id", sql, StringComparison.Ordinal);
        Assert.Equal(venueId, parameters!.GetType().GetProperty("VenueId")!.GetValue(parameters));
    }

    [Fact]
    public async Task GetCategoriesAsync_RejectsEmptyVenue()
    {
        var repository = new TapListRepository(new FakeSqlDataAccess());

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => repository.GetCategoriesAsync(Guid.Empty));

        Assert.Equal("venueId", exception.ParamName);
    }
}

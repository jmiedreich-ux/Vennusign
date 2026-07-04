using Vennu.Data.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public class ScreenRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task AssignVenueAsync_UpdatesExistingScreen()
    {
        var screen = new Screen
        {
            Id = Guid.NewGuid(),
            ScreenKey = "screen-1",
            Status = "Online"
        };

        var venueId = Guid.NewGuid();
        var dataAccess = new FakeSqlDataAccess
        {
            QueryHandler = _ => screen,
            UpdateResult = 1
        };

        var sut = new ScreenRepository(dataAccess);
        using var cancellationSource = new CancellationTokenSource();

        var updated = await sut.AssignVenueAsync(screen.Id, venueId, cancellationSource.Token);

        Assert.True(updated);
        Assert.Equal(venueId, screen.VenueId);
        Assert.NotEqual(default, screen.UpdatedUtc);
        Assert.Single(dataAccess.UpdatedEntities);
        Assert.Same(screen, dataAccess.UpdatedEntities[0]);
        Assert.Equal(cancellationSource.Token, dataAccess.LastCancellationToken);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetByVenueIdAsync_ReturnsScreensFromQuery()
    {
        var screens = new[]
        {
            new Screen { ScreenKey = "screen-1" },
            new Screen { ScreenKey = "screen-2" }
        };

        var dataAccess = new FakeSqlDataAccess
        {
            QueryManyHandler = _ => screens
        };

        var sut = new ScreenRepository(dataAccess);

        var results = await sut.GetByVenueIdAsync(Guid.NewGuid());

        Assert.Equal(2, results.Count);
        Assert.Equal(screens[0], results.First());
        Assert.Equal(screens[1], results.Last());
    }
}

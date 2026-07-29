using Vennu.Core.Models;
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
        var dataAccess = new FakeSqlDataAccess { QueryManyHandler = _ => screens };
        var sut = new ScreenRepository(dataAccess);

        var results = await sut.GetByVenueIdAsync(Guid.NewGuid());

        Assert.Equal(2, results.Count);
        Assert.Equal(screens[0], results.First());
        Assert.Equal(screens[1], results.Last());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateAsync_PersistsScreenAndRefreshesTimestamp()
    {
        var originalTimestamp = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var screen = new Screen { Id = Guid.NewGuid(), ScreenKey = "sc-test1", Name = "Patio", UpdatedUtc = originalTimestamp };
        var dataAccess = new FakeSqlDataAccess { UpdateResult = 1 };
        var sut = new ScreenRepository(dataAccess);

        var updated = await sut.UpdateAsync(screen);

        Assert.True(updated);
        Assert.True(screen.UpdatedUtc > originalTimestamp);
        Assert.Single(dataAccess.UpdatedEntities);
        Assert.Same(screen, dataAccess.UpdatedEntities[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task MarkStaleOnlineScreensOfflineAsync_OnlyUpdatesScreensOlderThanCutoff()
    {
        var cutoff = new DateTime(2026, 7, 25, 1, 0, 0, DateTimeKind.Utc);
        var stale = new Screen { Status = "Online", LastSeen = cutoff.AddTicks(-1), Name = "stale" };
        var boundary = new Screen { Status = "Online", LastSeen = cutoff, Name = "boundary" };
        var recent = new Screen { Status = "Online", LastSeen = cutoff.AddSeconds(1), Name = "recent" };
        var alreadyOffline = new Screen { Status = "Offline", LastSeen = cutoff.AddMinutes(-10), Name = "offline" };
        var dataAccess = new FakeSqlDataAccess
        {
            QueryAllHandler = _ => new object[] { stale, boundary, recent, alreadyOffline }
        };
        var sut = new ScreenRepository(dataAccess);

        var updated = await sut.MarkStaleOnlineScreensOfflineAsync(cutoff);

        Assert.Equal(1, updated);
        Assert.Equal("Offline", stale.Status);
        Assert.Equal("Online", boundary.Status);
        Assert.Equal("Online", recent.Status);
        Assert.Equal("Offline", alreadyOffline.Status);
        Assert.Single(dataAccess.UpdatedEntities);
        Assert.Same(stale, dataAccess.UpdatedEntities[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task MarkStaleOnlineScreensOfflineAsync_HandlesEmptyResultSet()
    {
        var dataAccess = new FakeSqlDataAccess { QueryAllHandler = _ => [] };
        var sut = new ScreenRepository(dataAccess);

        var updated = await sut.MarkStaleOnlineScreensOfflineAsync(DateTime.UtcNow);

        Assert.Equal(0, updated);
        Assert.Empty(dataAccess.UpdatedEntities);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task MarkStaleOnlineScreensOfflineAsync_DoesNotRepeatAlreadyCompletedTransition()
    {
        var cutoff = DateTime.UtcNow;
        var screen = new Screen { Status = "Online", LastSeen = cutoff.AddMinutes(-5) };
        var dataAccess = new FakeSqlDataAccess { QueryAllHandler = _ => new object[] { screen } };
        var sut = new ScreenRepository(dataAccess);

        var first = await sut.MarkStaleOnlineScreensOfflineAsync(cutoff);
        var second = await sut.MarkStaleOnlineScreensOfflineAsync(cutoff);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Single(dataAccess.UpdatedEntities);
    }
}

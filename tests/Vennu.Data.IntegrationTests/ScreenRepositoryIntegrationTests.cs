using Vennu.Data.IntegrationTests.Fixtures;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public class ScreenRepositoryIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture fixture;

    public ScreenRepositoryIntegrationTests(DatabaseFixture fixture) { this.fixture = fixture; }

    public Task InitializeAsync() => fixture.ResetTablesAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_PersistsScreenToDatabase()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);
        var screen = new Screen
        {
            ScreenKey = "sc-abc123",
            Name = "Test Screen",
            Platform = "web",
            AppVersion = "1.0.0",
            Status = "Offline"
        };

        var screenId = await sut.CreateAsync(screen);

        Assert.NotEqual(Guid.Empty, screenId);
        Assert.Equal(screenId, screen.Id);
        Assert.NotEqual(default, screen.CreatedUtc);
        Assert.NotEqual(default, screen.UpdatedUtc);
    }

    [Fact]
    public async Task GetByIdAsync_RetrievesPersistedScreen()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);
        var screen = new Screen
        {
            ScreenKey = "sc-def456",
            Name = "Readable Screen",
            Platform = "android",
            AppVersion = "2.0.0",
            Status = "Offline"
        };

        var screenId = await sut.CreateAsync(screen);
        var retrieved = await sut.GetByIdAsync(screenId);

        Assert.NotNull(retrieved);
        Assert.Equal(screenId, retrieved.Id);
        Assert.Equal("sc-def456", retrieved.ScreenKey);
        Assert.Equal("Readable Screen", retrieved.Name);
        Assert.Equal("android", retrieved.Platform);
        Assert.Equal("Offline", retrieved.Status);
    }

    [Fact]
    public async Task AssignVenueAsync_LinksScreenToVenue()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var venueRepo = new VenueRepository(dataAccess);
        var screenRepo = new ScreenRepository(dataAccess);

        var venueId = await venueRepo.CreateAsync(new Venue { Name = "Assign Venue", Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" });
        var screenId = await screenRepo.CreateAsync(new Screen { ScreenKey = "sc-ghi789", Name = "Link Screen", Status = "Offline" });

        var assigned = await screenRepo.AssignVenueAsync(screenId, venueId);

        Assert.True(assigned);

        var retrieved = await screenRepo.GetByIdAsync(screenId);
        Assert.NotNull(retrieved);
        Assert.Equal(venueId, retrieved.VenueId);
    }

    [Fact]
    public async Task UpdateHeartbeatAsync_UpdatesScreenStatus()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);
        var screenId = await sut.CreateAsync(new Screen { ScreenKey = "sc-jkl012", Name = "Heartbeat Screen", Status = "Offline" });

        var lastSeen = DateTime.UtcNow;
        var updated = await sut.UpdateHeartbeatAsync(screenId, lastSeen, "Online");

        Assert.True(updated);

        var retrieved = await sut.GetByIdAsync(screenId);
        Assert.NotNull(retrieved);
        Assert.Equal("Online", retrieved.Status);
        Assert.NotNull(retrieved.LastSeen);
    }

    [Fact]
    public async Task GetByVenueIdAsync_ReturnsScreensLinkedToVenue()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var venueRepo = new VenueRepository(dataAccess);
        var screenRepo = new ScreenRepository(dataAccess);

        var venueId = await venueRepo.CreateAsync(new Venue { Name = "Multi Screen Venue", Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" });
        var screenId1 = await screenRepo.CreateAsync(new Screen { ScreenKey = "sc-mno345", Name = "Screen 1", Status = "Offline" });
        var screenId2 = await screenRepo.CreateAsync(new Screen { ScreenKey = "sc-pqr678", Name = "Screen 2", Status = "Offline" });

        await screenRepo.AssignVenueAsync(screenId1, venueId);
        await screenRepo.AssignVenueAsync(screenId2, venueId);

        var screens = await screenRepo.GetByVenueIdAsync(venueId);

        Assert.Equal(2, screens.Count);
        Assert.Contains(screens, s => s.ScreenKey == "sc-mno345");
        Assert.Contains(screens, s => s.ScreenKey == "sc-pqr678");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenScreenDoesNotExist()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}

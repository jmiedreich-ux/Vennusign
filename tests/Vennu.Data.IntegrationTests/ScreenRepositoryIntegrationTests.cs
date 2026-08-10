using Vennu.Data.IntegrationTests.Fixtures;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public class ScreenRepositoryIntegrationTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{

    [Fact]
    public async Task CreateAsync_PersistsScreenToDatabase()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);
        var screenKey = fixture.UniqueScreenKey();
        var screenName = fixture.UniqueValue("test-screen");
        var screen = new Screen
        {
            ScreenKey = screenKey,
            Name = screenName,
            Platform = "web",
            AppVersion = "1.0.0",
            Status = "Offline"
        };

        var screenId = await sut.CreateAsync(screen);
        await fixture.TraceAsync(
            nameof(CreateAsync_PersistsScreenToDatabase),
            "Creates a standalone screen to prove ScreenRepository.CreateAsync persists screen registration fields.",
            "Screens",
            screenId.ToString(),
            "INSERT",
            new { screen.Id, screen.ScreenKey, screen.Name, screen.Platform, screen.AppVersion, screen.Status });

        Assert.NotEqual(Guid.Empty, screenId);
        Assert.Equal(screenId, screen.Id);
        Assert.NotEqual(default, screen.CreatedUtc);
        Assert.NotEqual(default, screen.UpdatedUtc);
    }

    [Fact]
    public async Task GetByIdAsync_RetrievesPersistedScreen()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);
        var screenKey = fixture.UniqueScreenKey();
        var screenName = fixture.UniqueValue("readable-screen");
        var screen = new Screen
        {
            ScreenKey = screenKey,
            Name = screenName,
            Platform = "android",
            AppVersion = "2.0.0",
            Status = "Offline"
        };

        var screenId = await sut.CreateAsync(screen);
        await fixture.TraceAsync(
            nameof(GetByIdAsync_RetrievesPersistedScreen),
            "Creates a screen that is immediately read back by Id to prove persisted screen data can be retrieved exactly.",
            "Screens",
            screenId.ToString(),
            "INSERT",
            new { screen.Id, screen.ScreenKey, screen.Name, screen.Platform, screen.AppVersion, screen.Status });
        var retrieved = await sut.GetByIdAsync(screenId);

        Assert.NotNull(retrieved);
        Assert.Equal(screenId, retrieved.Id);
        Assert.Equal(screenKey, retrieved.ScreenKey);
        Assert.Equal(screenName, retrieved.Name);
        Assert.Equal("android", retrieved.Platform);
        Assert.Equal("Offline", retrieved.Status);
    }

    [Fact]
    public async Task AssignVenueAsync_LinksScreenToVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var venueRepo = new VenueRepository(dataAccess);
        var screenRepo = new ScreenRepository(dataAccess);

        var venue = new Venue { Name = fixture.UniqueValue("assign-venue"), Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" };
        var screen = new Screen { ScreenKey = fixture.UniqueScreenKey(), Name = fixture.UniqueValue("link-screen"), Status = "Offline" };
        var venueId = await venueRepo.CreateAsync(venue);
        var screenId = await screenRepo.CreateAsync(screen);
        await fixture.TraceAsync(
            nameof(AssignVenueAsync_LinksScreenToVenue),
            "Creates a venue used as the target of a screen assignment.",
            "Venues",
            venueId.ToString(),
            "INSERT",
            new { venue.Id, venue.Name, venue.Timezone, venue.Type, venue.PrimaryLanguage });
        await fixture.TraceAsync(
            nameof(AssignVenueAsync_LinksScreenToVenue),
            "Creates an unassigned screen that will be linked to a venue.",
            "Screens",
            screenId.ToString(),
            "INSERT",
            new { screen.Id, screen.ScreenKey, screen.Name, screen.Status });

        var assigned = await screenRepo.AssignVenueAsync(screenId, venueId);
        await fixture.TraceAsync(
            nameof(AssignVenueAsync_LinksScreenToVenue),
            "Updates the screen VenueId to prove screen-to-venue linking is persisted.",
            "Screens",
            screenId.ToString(),
            "UPDATE",
            new { ScreenId = screenId, VenueId = venueId });

        Assert.True(assigned);

        var retrieved = await screenRepo.GetByIdAsync(screenId);
        Assert.NotNull(retrieved);
        Assert.Equal(venueId, retrieved.VenueId);
    }

    [Fact]
    public async Task UpdateHeartbeatAsync_UpdatesScreenStatus()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);
        var screen = new Screen { ScreenKey = fixture.UniqueScreenKey(), Name = fixture.UniqueValue("heartbeat-screen"), Status = "Offline" };
        var screenId = await sut.CreateAsync(screen);
        await fixture.TraceAsync(
            nameof(UpdateHeartbeatAsync_UpdatesScreenStatus),
            "Creates an offline screen that will receive a heartbeat update.",
            "Screens",
            screenId.ToString(),
            "INSERT",
            new { screen.Id, screen.ScreenKey, screen.Name, screen.Status });

        var lastSeen = DateTime.UtcNow;
        var updated = await sut.UpdateHeartbeatAsync(screenId, lastSeen, "Online");
        await fixture.TraceAsync(
            nameof(UpdateHeartbeatAsync_UpdatesScreenStatus),
            "Updates LastSeen and Status to prove heartbeat writes are persisted.",
            "Screens",
            screenId.ToString(),
            "UPDATE",
            new { ScreenId = screenId, LastSeen = lastSeen, Status = "Online" });

        Assert.True(updated);

        var retrieved = await sut.GetByIdAsync(screenId);
        Assert.NotNull(retrieved);
        Assert.Equal("Online", retrieved.Status);
        Assert.NotNull(retrieved.LastSeen);
    }

    [Fact]
    public async Task GetByVenueIdAsync_ReturnsScreensLinkedToVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var venueRepo = new VenueRepository(dataAccess);
        var screenRepo = new ScreenRepository(dataAccess);

        var screenKey1 = fixture.UniqueScreenKey();
        var screenKey2 = fixture.UniqueScreenKey();
        var venue = new Venue { Name = fixture.UniqueValue("multi-screen-venue"), Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" };
        var screen1 = new Screen { ScreenKey = screenKey1, Name = fixture.UniqueValue("screen-1"), Status = "Offline" };
        var screen2 = new Screen { ScreenKey = screenKey2, Name = fixture.UniqueValue("screen-2"), Status = "Offline" };
        var venueId = await venueRepo.CreateAsync(venue);
        var screenId1 = await screenRepo.CreateAsync(screen1);
        var screenId2 = await screenRepo.CreateAsync(screen2);
        await fixture.TraceAsync(
            nameof(GetByVenueIdAsync_ReturnsScreensLinkedToVenue),
            "Creates a venue used to validate lookup of all linked screens.",
            "Venues",
            venueId.ToString(),
            "INSERT",
            new { venue.Id, venue.Name, venue.Timezone, venue.Type, venue.PrimaryLanguage });
        await fixture.TraceAsync(
            nameof(GetByVenueIdAsync_ReturnsScreensLinkedToVenue),
            "Creates the first screen that will be assigned to the lookup venue.",
            "Screens",
            screenId1.ToString(),
            "INSERT",
            new { screen1.Id, screen1.ScreenKey, screen1.Name, screen1.Status });
        await fixture.TraceAsync(
            nameof(GetByVenueIdAsync_ReturnsScreensLinkedToVenue),
            "Creates the second screen that will be assigned to the lookup venue.",
            "Screens",
            screenId2.ToString(),
            "INSERT",
            new { screen2.Id, screen2.ScreenKey, screen2.Name, screen2.Status });

        await screenRepo.AssignVenueAsync(screenId1, venueId);
        await screenRepo.AssignVenueAsync(screenId2, venueId);
        await fixture.TraceAsync(
            nameof(GetByVenueIdAsync_ReturnsScreensLinkedToVenue),
            "Updates both screens with the same VenueId to prove GetByVenueIdAsync returns the linked set.",
            "Screens",
            $"{screenId1},{screenId2}",
            "UPDATE",
            new[] { new { ScreenId = screenId1, VenueId = venueId }, new { ScreenId = screenId2, VenueId = venueId } });

        var screens = await screenRepo.GetByVenueIdAsync(venueId);

        Assert.Equal(2, screens.Count);
        Assert.Contains(screens, s => s.ScreenKey == screenKey1);
        Assert.Contains(screens, s => s.ScreenKey == screenKey2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenScreenDoesNotExist()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenRepository(dataAccess);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}

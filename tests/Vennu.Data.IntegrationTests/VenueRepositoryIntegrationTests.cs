using Vennu.Data.IntegrationTests.Fixtures;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public class VenueRepositoryIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture fixture;

    public VenueRepositoryIntegrationTests(DatabaseFixture fixture) { this.fixture = fixture; }

    public Task InitializeAsync() => fixture.ResetTablesAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_PersistsVenueToDatabase()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);
        var venue = new Venue
        {
            Name = "Test Venue",
            Timezone = "America/New_York",
            Type = "Bar",
            PrimaryLanguage = "en",
            SecondaryLanguage = "es"
        };

        var venueId = await sut.CreateAsync(venue);

        Assert.NotEqual(Guid.Empty, venueId);
        Assert.Equal(venueId, venue.Id);
        Assert.NotEqual(default, venue.CreatedUtc);
        Assert.NotEqual(default, venue.UpdatedUtc);
    }

    [Fact]
    public async Task GetByIdAsync_RetrievesPersistedVenue()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);
        var venue = new Venue
        {
            Name = "Readable Venue",
            Timezone = "UTC",
            Type = "Club",
            PrimaryLanguage = "en"
        };

        var venueId = await sut.CreateAsync(venue);
        var retrieved = await sut.GetByIdAsync(venueId);

        Assert.NotNull(retrieved);
        Assert.Equal(venueId, retrieved.Id);
        Assert.Equal("Readable Venue", retrieved.Name);
        Assert.Equal("UTC", retrieved.Timezone);
        Assert.Equal("Club", retrieved.Type);
        Assert.Equal("en", retrieved.PrimaryLanguage);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedVenues()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);

        await sut.CreateAsync(new Venue { Name = "Venue A", Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" });
        await sut.CreateAsync(new Venue { Name = "Venue B", Timezone = "UTC", Type = "Club", PrimaryLanguage = "fr" });

        var all = await sut.GetAllAsync();

        Assert.True(all.Count >= 2);
        Assert.Contains(all, v => v.Name == "Venue A");
        Assert.Contains(all, v => v.Name == "Venue B");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenVenueDoesNotExist()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}

using Vennu.Data.IntegrationTests.Fixtures;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public class VenueRepositoryIntegrationTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{

    [Fact]
    public async Task CreateAsync_PersistsVenueToDatabase()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);
        var venueName = fixture.UniqueValue("test-venue");
        var venue = new Venue
        {
            Name = venueName,
            Timezone = "America/New_York",
            Type = "Bar",
            PrimaryLanguage = "en",
            SecondaryLanguage = "es"
        };

        var venueId = await sut.CreateAsync(venue);
        await fixture.TraceAsync(
            nameof(CreateAsync_PersistsVenueToDatabase),
            "Creates a standalone venue to prove VenueRepository.CreateAsync persists required venue fields.",
            "Venues",
            venueId.ToString(),
            "INSERT",
            new { venue.Id, venue.Name, venue.Timezone, venue.Type, venue.PrimaryLanguage, venue.SecondaryLanguage });

        Assert.NotEqual(Guid.Empty, venueId);
        Assert.Equal(venueId, venue.Id);
        Assert.NotEqual(default, venue.CreatedUtc);
        Assert.NotEqual(default, venue.UpdatedUtc);
    }

    [Fact]
    public async Task GetByIdAsync_RetrievesPersistedVenue()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);
        var venueName = fixture.UniqueValue("readable-venue");
        var venue = new Venue
        {
            Name = venueName,
            Timezone = "UTC",
            Type = "Club",
            PrimaryLanguage = "en"
        };

        var venueId = await sut.CreateAsync(venue);
        await fixture.TraceAsync(
            nameof(GetByIdAsync_RetrievesPersistedVenue),
            "Creates a venue that is immediately read back by Id to prove persisted data can be retrieved exactly.",
            "Venues",
            venueId.ToString(),
            "INSERT",
            new { venue.Id, venue.Name, venue.Timezone, venue.Type, venue.PrimaryLanguage });
        var retrieved = await sut.GetByIdAsync(venueId);

        Assert.NotNull(retrieved);
        Assert.Equal(venueId, retrieved.Id);
        Assert.Equal(venueName, retrieved.Name);
        Assert.Equal("UTC", retrieved.Timezone);
        Assert.Equal("Club", retrieved.Type);
        Assert.Equal("en", retrieved.PrimaryLanguage);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedVenues()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);
        var venueNameA = fixture.UniqueValue("venue-a");
        var venueNameB = fixture.UniqueValue("venue-b");

        var venueA = new Venue { Name = venueNameA, Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" };
        var venueB = new Venue { Name = venueNameB, Timezone = "UTC", Type = "Club", PrimaryLanguage = "fr" };
        var venueIdA = await sut.CreateAsync(venueA);
        var venueIdB = await sut.CreateAsync(venueB);
        await fixture.TraceAsync(
            nameof(GetAllAsync_ReturnsAllPersistedVenues),
            "Creates the first uniquely named venue used to prove GetAllAsync includes newly persisted records among existing dev data.",
            "Venues",
            venueIdA.ToString(),
            "INSERT",
            new { venueA.Id, venueA.Name, venueA.Timezone, venueA.Type, venueA.PrimaryLanguage });
        await fixture.TraceAsync(
            nameof(GetAllAsync_ReturnsAllPersistedVenues),
            "Creates the second uniquely named venue used to prove GetAllAsync includes multiple new records among existing dev data.",
            "Venues",
            venueIdB.ToString(),
            "INSERT",
            new { venueB.Id, venueB.Name, venueB.Timezone, venueB.Type, venueB.PrimaryLanguage });

        var all = await sut.GetAllAsync();

        Assert.True(all.Count >= 2);
        Assert.Contains(all, v => v.Name == venueNameA);
        Assert.Contains(all, v => v.Name == venueNameB);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenVenueDoesNotExist()
    {
        var dataAccess = fixture.CreateDataAccess();
        var sut = new VenueRepository(dataAccess);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}

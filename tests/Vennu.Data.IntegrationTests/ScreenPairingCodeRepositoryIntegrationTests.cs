using Vennu.Data.IntegrationTests.Fixtures;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public class ScreenPairingCodeRepositoryIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture fixture;

    public ScreenPairingCodeRepositoryIntegrationTests(DatabaseFixture fixture) { this.fixture = fixture; }

    public Task InitializeAsync() => fixture.ResetTablesAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_PersistsPairingCodeToDatabase()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var screenRepo = new ScreenRepository(dataAccess);
        var sut = new ScreenPairingCodeRepository(dataAccess);
        var pairingCodeValue = fixture.UniqueCode();

        var screen = new Screen { ScreenKey = fixture.UniqueScreenKey(), Name = fixture.UniqueValue("pairing-screen"), Status = "Offline" };
        var screenId = await screenRepo.CreateAsync(screen);
        await fixture.TraceAsync(
            nameof(CreateAsync_PersistsPairingCodeToDatabase),
            "Creates the screen required by the pairing-code foreign key before inserting a pairing code.",
            "Screens",
            screenId.ToString(),
            "INSERT",
            new { screen.Id, screen.ScreenKey, screen.Name, screen.Status });
        var pairingCode = new ScreenPairingCode
        {
            Code = pairingCodeValue,
            ScreenId = screenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsClaimed = false
        };

        var code = await sut.CreateAsync(pairingCode);
        await fixture.TraceAsync(
            nameof(CreateAsync_PersistsPairingCodeToDatabase),
            "Creates an unclaimed pairing code to prove ScreenPairingCodeRepository.CreateAsync persists code rows.",
            "ScreenPairingCodes",
            code,
            "INSERT",
            new { pairingCode.Code, pairingCode.ScreenId, pairingCode.ExpiresAt, pairingCode.IsClaimed, pairingCode.CreatedUtc });

        Assert.Equal(pairingCodeValue, code);
        Assert.NotEqual(default, pairingCode.CreatedUtc);
    }

    [Fact]
    public async Task GetByCodeAsync_RetrievesPersistedPairingCode()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var screenRepo = new ScreenRepository(dataAccess);
        var sut = new ScreenPairingCodeRepository(dataAccess);
        var pairingCodeValue = fixture.UniqueCode();

        var screen = new Screen { ScreenKey = fixture.UniqueScreenKey(), Name = fixture.UniqueValue("read-screen"), Status = "Offline" };
        var screenId = await screenRepo.CreateAsync(screen);
        await fixture.TraceAsync(
            nameof(GetByCodeAsync_RetrievesPersistedPairingCode),
            "Creates the screen required by the pairing-code foreign key before creating a code to read back.",
            "Screens",
            screenId.ToString(),
            "INSERT",
            new { screen.Id, screen.ScreenKey, screen.Name, screen.Status });
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var pairingCode = new ScreenPairingCode
        {
            Code = pairingCodeValue,
            ScreenId = screenId,
            ExpiresAt = expiresAt,
            IsClaimed = false
        };
        await sut.CreateAsync(pairingCode);
        await fixture.TraceAsync(
            nameof(GetByCodeAsync_RetrievesPersistedPairingCode),
            "Creates a pairing code with known values so GetByCodeAsync can prove the same row is retrieved.",
            "ScreenPairingCodes",
            pairingCodeValue,
            "INSERT",
            new { pairingCode.Code, pairingCode.ScreenId, pairingCode.ExpiresAt, pairingCode.IsClaimed, pairingCode.CreatedUtc });

        var retrieved = await sut.GetByCodeAsync(pairingCodeValue);

        Assert.NotNull(retrieved);
        Assert.Equal(pairingCodeValue, retrieved.Code);
        Assert.Equal(screenId, retrieved.ScreenId);
        Assert.False(retrieved.IsClaimed);
    }

    [Fact]
    public async Task ClaimAsync_MarksPairingCodeAsClaimed()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var venueRepo = new VenueRepository(dataAccess);
        var screenRepo = new ScreenRepository(dataAccess);
        var sut = new ScreenPairingCodeRepository(dataAccess);
        var pairingCodeValue = fixture.UniqueCode();

        var venue = new Venue { Name = fixture.UniqueValue("claim-venue"), Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" };
        var screen = new Screen { ScreenKey = fixture.UniqueScreenKey(), Name = fixture.UniqueValue("claim-screen"), Status = "Offline" };
        var venueId = await venueRepo.CreateAsync(venue);
        var screenId = await screenRepo.CreateAsync(screen);
        await fixture.TraceAsync(
            nameof(ClaimAsync_MarksPairingCodeAsClaimed),
            "Creates the venue that will claim the pairing code.",
            "Venues",
            venueId.ToString(),
            "INSERT",
            new { venue.Id, venue.Name, venue.Timezone, venue.Type, venue.PrimaryLanguage });
        await fixture.TraceAsync(
            nameof(ClaimAsync_MarksPairingCodeAsClaimed),
            "Creates the screen that will be linked to the claiming venue through the pairing code.",
            "Screens",
            screenId.ToString(),
            "INSERT",
            new { screen.Id, screen.ScreenKey, screen.Name, screen.Status });
        var pairingCode = new ScreenPairingCode
        {
            Code = pairingCodeValue,
            ScreenId = screenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsClaimed = false
        };
        await sut.CreateAsync(pairingCode);
        await fixture.TraceAsync(
            nameof(ClaimAsync_MarksPairingCodeAsClaimed),
            "Creates an unclaimed pairing code so ClaimAsync can prove claim state, venue link, and ClaimedAt are persisted.",
            "ScreenPairingCodes",
            pairingCodeValue,
            "INSERT",
            new { pairingCode.Code, pairingCode.ScreenId, pairingCode.ExpiresAt, pairingCode.IsClaimed, pairingCode.CreatedUtc });

        var claimed = await sut.ClaimAsync(pairingCodeValue, venueId);
        await fixture.TraceAsync(
            nameof(ClaimAsync_MarksPairingCodeAsClaimed),
            "Claims the pairing code by setting VenueId, IsClaimed, and ClaimedAt.",
            "ScreenPairingCodes",
            pairingCodeValue,
            "UPDATE",
            new { Code = pairingCodeValue, VenueId = venueId, ScreenId = screenId, IsClaimed = true });

        Assert.True(claimed);

        var retrieved = await sut.GetByCodeAsync(pairingCodeValue);
        Assert.NotNull(retrieved);
        Assert.True(retrieved.IsClaimed);
        Assert.Equal(venueId, retrieved.VenueId);
        Assert.NotNull(retrieved.ClaimedAt);
    }

    [Fact]
    public async Task ClaimAsync_ReturnsFalse_WhenCodeDoesNotExist()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenPairingCodeRepository(dataAccess);

        var claimed = await sut.ClaimAsync("000000", Guid.NewGuid());

        Assert.False(claimed);
    }

    [Fact]
    public async Task GetByCodeAsync_ReturnsNull_WhenCodeDoesNotExist()
    {
        if (!fixture.IsAvailable)
        {
            return;
        }

        var dataAccess = fixture.CreateDataAccess();
        var sut = new ScreenPairingCodeRepository(dataAccess);

        var result = await sut.GetByCodeAsync("ZZZZZZ");

        Assert.Null(result);
    }
}

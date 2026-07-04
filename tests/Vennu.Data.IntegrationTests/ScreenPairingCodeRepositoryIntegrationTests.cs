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

        var screenId = await screenRepo.CreateAsync(new Screen { ScreenKey = "sc-abc001", Name = "Pairing Screen", Status = "Offline" });
        var pairingCode = new ScreenPairingCode
        {
            Code = "123456",
            ScreenId = screenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsClaimed = false
        };

        var code = await sut.CreateAsync(pairingCode);

        Assert.Equal("123456", code);
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

        var screenId = await screenRepo.CreateAsync(new Screen { ScreenKey = "sc-abc002", Name = "Read Screen", Status = "Offline" });
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        await sut.CreateAsync(new ScreenPairingCode
        {
            Code = "654321",
            ScreenId = screenId,
            ExpiresAt = expiresAt,
            IsClaimed = false
        });

        var retrieved = await sut.GetByCodeAsync("654321");

        Assert.NotNull(retrieved);
        Assert.Equal("654321", retrieved.Code);
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

        var venueId = await venueRepo.CreateAsync(new Venue { Name = "Claim Venue", Timezone = "UTC", Type = "Bar", PrimaryLanguage = "en" });
        var screenId = await screenRepo.CreateAsync(new Screen { ScreenKey = "sc-abc003", Name = "Claim Screen", Status = "Offline" });
        await sut.CreateAsync(new ScreenPairingCode
        {
            Code = "999888",
            ScreenId = screenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsClaimed = false
        });

        var claimed = await sut.ClaimAsync("999888", venueId);

        Assert.True(claimed);

        var retrieved = await sut.GetByCodeAsync("999888");
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

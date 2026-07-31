using Vennu.Api.Contracts.Admin;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Services;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class HaasPreRegistrationServiceTests
{
    [Fact]
    public async Task CreateAsync_StoresOnlyHashedOneTimeToken()
    {
        var venueId = Guid.NewGuid();
        var screens = new FakeScreenRepository();
        var service = CreateService(venueId, screens);

        var result = await service.CreateAsync(venueId, new HaasPreRegistrationRequest(
            "Lobby TV", "Entry", "android_tv", "2.1.0", "shipment-42", 24));

        Assert.Equal(64, result.BootstrapToken.Length);
        Assert.Equal("/provision", result.LaunchPath);
        Assert.NotNull(screens.LastCreatedScreen);
        Assert.NotEqual(result.BootstrapToken, screens.LastCreatedScreen!.PreRegistrationTokenHash);
        Assert.Equal(HaasPreRegistrationService.Hash(result.BootstrapToken), screens.LastCreatedScreen.PreRegistrationTokenHash);
        Assert.Equal("shipment-42", screens.LastCreatedScreen.DeliveryReference);
    }

    [Fact]
    public async Task ClaimAsync_ConsumesValidMatchingPlatformToken()
    {
        var venueId = Guid.NewGuid();
        const string token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var screen = new Screen
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            ScreenKey = "sc-abc123",
            Platform = "webos",
            PreRegistrationTokenHash = HaasPreRegistrationService.Hash(token),
            PreRegistrationExpiresUtc = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var screens = new FakeScreenRepository
        {
            GetByPreRegistrationTokenHashAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen),
            ClaimPreRegisteredAsyncHandler = (id, platform, version, _, _) =>
                Task.FromResult(id == screen.Id && platform == "webos" && version == "3.0.0")
        };
        var service = CreateService(venueId, screens);

        var result = await service.ClaimAsync(new ClaimPreRegisteredScreenRequest
        {
            Token = token,
            Platform = "webos",
            AppVersion = "3.0.0"
        });

        Assert.Equal(screen.Id, result?.ScreenId);
        Assert.Equal($"/display/{screen.Id}", result?.DisplayPath);
    }

    [Fact]
    public async Task ClaimAsync_RejectsExpiredOrWrongPlatformToken()
    {
        var venueId = Guid.NewGuid();
        var screen = new Screen
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Platform = "tizen",
            PreRegistrationExpiresUtc = new DateTime(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc)
        };
        var screens = new FakeScreenRepository
        {
            GetByPreRegistrationTokenHashAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen)
        };
        var service = CreateService(venueId, screens);

        var result = await service.ClaimAsync(new ClaimPreRegisteredScreenRequest
        {
            Token = new string('b', 32),
            Platform = "webos",
            AppVersion = "1.0"
        });

        Assert.Null(result);
    }

    private static HaasPreRegistrationService CreateService(Guid venueId, FakeScreenRepository screens) =>
        new(
            screens,
            new FakeVenueRepository
            {
                GetByIdAsyncHandler = (id, _) => Task.FromResult<Venue?>(id == venueId ? new Venue { Id = id } : null)
            },
            new FixedTimeProvider());

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 30, 23, 15, 0, TimeSpan.Zero);
    }
}

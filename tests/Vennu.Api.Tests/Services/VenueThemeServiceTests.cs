using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Api.Notifications;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class VenueThemeServiceTests
{
    private readonly Guid venueId = Guid.NewGuid();

    [Fact]
    public async Task GetAsync_ReturnsDeterministicDefaults_WhenThemeIsNotPersisted()
    {
        var service = CreateService();

        var result = await service.GetAsync(venueId);

        Assert.Equal("#111315", result.BackgroundColor);
        Assert.Equal("#FFB74D", result.AccentColor);
        Assert.Equal("Inter", result.FontFamily);
    }

    [Fact]
    public async Task UpdateAsync_NormalizesAndPersistsValidatedTheme()
    {
        var themes = new ThemeRepository();
        var service = CreateService(themes);

        var result = await service.UpdateAsync(venueId, " #aabbcc ", "#010203", " georgia ");

        Assert.Equal("#AABBCC", result.BackgroundColor);
        Assert.Equal("Georgia", result.FontFamily);
        Assert.Equal(result, await service.GetAsync(venueId));
    }

    [Fact]
    public async Task UpdateAsync_NotifiesEveryVenueScreenThroughThemeChannel()
    {
        var notifier = new ThemeNotifier();
        var service = new VenueThemeService(
            new VenueRepository(new Venue { Id = venueId }),
            new ThemeRepository(),
            TimeProvider.System,
            notifier);

        await service.UpdateAsync(venueId, "#010203", "#AABBCC", "Arial");

        Assert.Equal(venueId, notifier.VenueId);
        Assert.NotNull(notifier.Theme);
    }

    [Theory]
    [InlineData("red", "#010203", "Inter")]
    [InlineData("#010203", "#XYZXYZ", "Inter")]
    [InlineData("#010203", "#AABBCC", "Comic Sans")]
    public async Task UpdateAsync_RejectsInvalidValues(string background, string accent, string font)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(venueId, background, accent, font));
    }

    [Fact]
    public async Task GetAsync_RejectsUnknownVenue()
    {
        var service = new VenueThemeService(
            new VenueRepository(null),
            new ThemeRepository(),
            TimeProvider.System);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAsync(venueId));
    }

    private VenueThemeService CreateService(ThemeRepository? themes = null) =>
        new(
            new VenueRepository(new Venue { Id = venueId }),
            themes ?? new ThemeRepository(),
            TimeProvider.System);

    private sealed class VenueRepository(Venue? venue) : IVenueRepository
    {
        public Task<Guid> CreateAsync(Venue value, CancellationToken cancellationToken = default) =>
            Task.FromResult(value.Id);

        public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Venue>>(venue is null ? [] : [venue]);

        public Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(venue?.Id == id ? venue : null);
    }

    private sealed class ThemeRepository : IVenueThemeRepository
    {
        private VenueTheme? theme;

        public Task<VenueTheme?> GetByVenueIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(theme?.VenueId == id ? theme : null);

        public Task UpsertAsync(VenueTheme value, CancellationToken cancellationToken = default)
        {
            theme = value;
            return Task.CompletedTask;
        }
    }

    private sealed class ThemeNotifier : IScreenUpdateNotifier
    {
        public Guid? VenueId { get; private set; }
        public object? Theme { get; private set; }

        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default)
        {
            VenueId = venueId;
            Theme = theme;
            return Task.CompletedTask;
        }

        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

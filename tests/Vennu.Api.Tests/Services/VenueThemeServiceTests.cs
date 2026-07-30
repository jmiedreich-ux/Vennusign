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
        Assert.Equal("bar_classic", result.PresetKey);
        Assert.Equal(4, result.SectionColors.Count);
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
    public async Task ApplyPresetAsync_UsesFiveDeterministicRoadmapPresets()
    {
        var service = CreateService();

        Assert.Equal(5, VenueThemePresets.GetAll().Count);
        var result = await service.ApplyPresetAsync(venueId, "violet_lounge");

        Assert.Equal("violet_lounge", result.PresetKey);
        Assert.Equal("#A855F7", result.GlowColor);
        Assert.Equal("Pacifico", result.TitleFont);
        Assert.Equal(4, result.SectionColors.Count);
    }

    [Fact]
    public async Task UpdateAdvancedAsync_NormalizesCustomValues_WithoutChangingBasicTheme()
    {
        var themes = new ThemeRepository();
        var service = CreateService(themes);
        await service.UpdateAsync(venueId, "#101010", "#202020", "Georgia");

        var result = await service.UpdateAdvancedAsync(
            venueId,
            new("#aabbcc", "#010203", "#040506", ["#111111", "#222222"], 1.75m, " permanent marker ", "kalam"));

        Assert.Equal("custom", result.PresetKey);
        Assert.Equal("#AABBCC", result.TitleColor);
        Assert.Equal(1.75m, result.GlowIntensity);
        Assert.Equal("Permanent Marker", result.TitleFont);
        Assert.Equal("Kalam", result.ItemFont);
        Assert.Equal("#101010", result.BackgroundColor);
        Assert.Equal("Georgia", result.FontFamily);
    }

    [Theory]
    [InlineData(0.19)]
    [InlineData(2.01)]
    public async Task UpdateAdvancedAsync_RejectsOutOfRangeGlow(double glow)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.UpdateAdvancedAsync(
                venueId,
                new("#AABBCC", "#010203", "#040506", ["#111111"], (decimal)glow, "Righteous", "Caveat")));
    }

    [Fact]
    public async Task UpdateAdvancedAsync_RejectsMoreThanFourSectionColors()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateAdvancedAsync(
                venueId,
                new("#AABBCC", "#010203", "#040506", ["#111111", "#222222", "#333333", "#444444", "#555555"], 1m, "Righteous", "Caveat")));
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

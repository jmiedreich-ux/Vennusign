using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

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
}

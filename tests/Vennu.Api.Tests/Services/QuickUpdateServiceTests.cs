using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;
using Vennu.Api.Tests.TestDoubles;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class QuickUpdateServiceTests
{
    [Fact]
    public async Task SetAvailabilityAsync_UsesVenueLocalMidnightAndNotifies()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var menus = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = [new MenuItem { Id = itemId, VenueId = venueId, MenuSectionId = sectionId, Name = "Burger", IsAvailable = true }]
        };
        var notifier = new RecordingNotifier();
        var service = new QuickUpdateService(
            menus,
            new FakeVenueRepository(new Venue { Id = venueId, Name = "Cafe", Timezone = "America/New_York" }),
            new FakeCapabilityDecisionServices("content.item.update", "content.item.availability_update"),
            notifier,
            new FixedTimeProvider());

        var updated = await service.SetAvailabilityAsync(venueId, menuId, sectionId, itemId, false);

        Assert.NotNull(updated);
        Assert.False(updated.IsAvailable);
        Assert.Equal(new DateTime(2026, 7, 30, 4, 0, 0, DateTimeKind.Utc), updated.AvailabilityResetUtc);
        Assert.Equal(1, notifier.AvailabilityCount);
        Assert.Equal(1, notifier.ContentCount);
    }

    [Fact]
    public async Task UpdateDailySpecialAsync_NormalizesAndNotifies()
    {
        var venueId = Guid.NewGuid();
        var menu = new Menu { Id = Guid.NewGuid(), VenueId = venueId, Name = "Main" };
        var menus = new FakeMenuRepository { Menus = [menu] };
        var notifier = new RecordingNotifier();
        var service = new QuickUpdateService(
            menus,
            new FakeVenueRepository(new Venue { Id = venueId, Name = "Cafe" }),
            new FakeCapabilityDecisionServices("content.item.update", "content.item.availability_update"),
            notifier,
            new FixedTimeProvider());

        var updated = await service.UpdateDailySpecialAsync(venueId, menu.Id, "  Smoked brisket tacos  ");

        Assert.Equal("Smoked brisket tacos", updated?.DailySpecial);
        Assert.Equal(menu, menus.UpdatedMenu);
        Assert.Equal(1, notifier.ContentCount);
    }

    [Fact]
    public async Task SetAvailabilityAsync_DoesNotUpdateArchivedItem()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var menus = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = [new MenuItem { Id = itemId, VenueId = venueId, MenuSectionId = sectionId, Name = "Archived", IsActive = false }]
        };
        var notifier = new RecordingNotifier();
        var service = new QuickUpdateService(
            menus,
            new FakeVenueRepository(new Venue { Id = venueId, Name = "Cafe" }),
            new FakeCapabilityDecisionServices("content.item.update", "content.item.availability_update"),
            notifier,
            new FixedTimeProvider());

        var updated = await service.SetAvailabilityAsync(venueId, menuId, sectionId, itemId, false);

        Assert.Null(updated);
        Assert.Equal(0, notifier.AvailabilityCount);
        Assert.Equal(0, notifier.ContentCount);
    }

    [Fact]
    public async Task QuickUpdate_BlocksWhenServerDecisionIsDenied()
    {
        var service = new QuickUpdateService(
            new FakeMenuRepository(),
            new FakeVenueRepository(null),
            new FakeCapabilityDecisionServices(),
            new RecordingNotifier(),
            new FixedTimeProvider());

        var error = await Assert.ThrowsAsync<CapabilityDecisionDeniedException>(
            () => service.UpdateDailySpecialAsync(Guid.NewGuid(), Guid.NewGuid(), "Special"));

        Assert.Equal("permission.required", error.Decision.ReasonCode);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 29, 23, 30, 0, TimeSpan.Zero);
    }

    private sealed class FakeVenueRepository(Venue? venue) : IVenueRepository
    {
        public Task<Guid> CreateAsync(Venue value, CancellationToken cancellationToken = default) => Task.FromResult(value.Id);
        public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Venue>>(venue is null ? [] : [venue]);
        public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(venue?.Id == venueId ? venue : null);
    }

    private sealed class FakeMenuRepository : IMenuRepository
    {
        public IReadOnlyCollection<Menu> Menus { get; init; } = [];
        public IReadOnlyCollection<MenuSection> Sections { get; init; } = [];
        public IReadOnlyCollection<MenuItem> Items { get; init; } = [];
        public Menu? UpdatedMenu { get; private set; }
        public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => Task.FromResult(menu.Id);
        public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => Task.FromResult(section.Id);
        public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => Task.FromResult(item.Id);
        public Task<Guid> CreateTranslationAsync(MenuItemTranslation translation, CancellationToken cancellationToken = default) => Task.FromResult(translation.Id);
        public Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateMenuAsync(Menu menu, CancellationToken cancellationToken = default)
        {
            UpdatedMenu = menu;
            return Task.FromResult(true);
        }
        public Task<IReadOnlyCollection<RestoredMenuItem>> RestoreExpiredAvailabilityAsync(DateTime utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<RestoredMenuItem>>([]);
        public Task<int> ReorderSectionsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, DateTime updatedUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult(sectionIds.Count);
        public Task<IReadOnlyCollection<Menu>> GetMenusAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Menu>>(Menus.Where(menu => menu.VenueId == venueId).ToArray());
        public Task<IReadOnlyCollection<MenuSection>> GetSectionsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<MenuSection>>(Sections.Where(section => section.VenueId == venueId && section.MenuId == menuId).ToArray());
        public Task<IReadOnlyCollection<MenuItem>> GetItemsAsync(Guid venueId, Guid sectionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<MenuItem>>(Items.Where(item => item.VenueId == venueId && item.MenuSectionId == sectionId).ToArray());
        public Task<IReadOnlyCollection<MenuItemTranslation>> GetTranslationsAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<MenuItemTranslation>>([]);
    }

    private sealed class RecordingNotifier : IScreenUpdateNotifier
    {
        public int AvailabilityCount { get; private set; }
        public int ContentCount { get; private set; }
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) { ContentCount++; return Task.CompletedTask; }
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) { AvailabilityCount++; return Task.CompletedTask; }
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

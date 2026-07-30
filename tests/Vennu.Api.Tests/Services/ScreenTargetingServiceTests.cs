using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class ScreenTargetingServiceTests
{
    [Fact]
    public async Task PushAllAsync_NotifiesVenueOnceAndReportsOwnedScreenCount()
    {
        var venueId = Guid.NewGuid();
        var screens = new FakeScreenRepository
        {
            GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>(
            [
                new Screen { Id = Guid.NewGuid(), VenueId = venueId },
                new Screen { Id = Guid.NewGuid(), VenueId = venueId }
            ])
        };
        var notifier = new RecordingNotifier();
        var service = CreateService(venueId, screens, new FakeMenuRepository(), notifier);

        var count = await service.PushAllAsync(venueId);

        Assert.Equal(2, count);
        Assert.Equal(venueId, notifier.VenueId);
        Assert.Equal(1, notifier.VenueContentCount);
        Assert.Equal(0, notifier.ScreenContentCount);
    }

    [Fact]
    public async Task GetOverflowAsync_UsesStableSectionAndItemOrder()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var firstSection = Guid.NewGuid();
        var secondSection = Guid.NewGuid();
        var menus = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main", IsActive = true }],
            Sections =
            [
                new MenuSection { Id = secondSection, VenueId = venueId, MenuId = menuId, Name = "Drinks", SortOrder = 2, IsActive = true },
                new MenuSection { Id = firstSection, VenueId = venueId, MenuId = menuId, Name = "Food", SortOrder = 1, IsActive = true }
            ],
            Items =
            [
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = firstSection, Name = "Second", SortOrder = 2, IsAvailable = true },
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = secondSection, Name = "Fifth", SortOrder = 1, IsAvailable = true },
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = firstSection, Name = "First", SortOrder = 1, IsAvailable = true },
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = firstSection, Name = "Hidden", SortOrder = 3, IsAvailable = false },
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = firstSection, Name = "Third", SortOrder = 3, IsAvailable = true },
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = firstSection, Name = "Fourth", SortOrder = 4, IsAvailable = true }
            ]
        };
        var service = CreateService(venueId, new FakeScreenRepository(), menus, new RecordingNotifier());

        var preview = await service.GetOverflowAsync(venueId, 4);

        Assert.Equal(5, preview.TotalItems);
        Assert.Equal(4, preview.VisibleItems);
        Assert.Equal(1, preview.OverflowItems);
        Assert.Equal(["First", "Second", "Third", "Fourth", "Fifth"], preview.Items.Select(item => item.ItemName));
        Assert.Equal([true, true, true, true, false], preview.Items.Select(item => item.Visible));
    }

    [Fact]
    public async Task GetOverflowAsync_RejectsUnsupportedCapacity()
    {
        var venueId = Guid.NewGuid();
        var service = CreateService(venueId, new FakeScreenRepository(), new FakeMenuRepository(), new RecordingNotifier());

        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.GetOverflowAsync(venueId, 5));

        Assert.Contains("4, 6, 8, or 9", error.Message);
    }

    private static ScreenTargetingService CreateService(
        Guid venueId,
        FakeScreenRepository screens,
        FakeMenuRepository menus,
        RecordingNotifier notifier) =>
        new(
            screens,
            new FakeVenueRepository
            {
                GetByIdAsyncHandler = (id, _) => Task.FromResult<Venue?>(
                    id == venueId ? new Venue { Id = id, Name = "Cafe" } : null)
            },
            menus,
            notifier,
            new FixedTimeProvider());

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 30, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeMenuRepository : IMenuRepository
    {
        public IReadOnlyCollection<Menu> Menus { get; init; } = [];
        public IReadOnlyCollection<MenuSection> Sections { get; init; } = [];
        public IReadOnlyCollection<MenuItem> Items { get; init; } = [];
        public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => Task.FromResult(menu.Id);
        public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => Task.FromResult(section.Id);
        public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => Task.FromResult(item.Id);
        public Task<Guid> CreateTranslationAsync(MenuItemTranslation translation, CancellationToken cancellationToken = default) => Task.FromResult(translation.Id);
        public Task<bool> UpdateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<int> ReorderSectionsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, DateTime updatedUtc, CancellationToken cancellationToken = default) => Task.FromResult(sectionIds.Count);
        public Task<IReadOnlyCollection<RestoredMenuItem>> RestoreExpiredAvailabilityAsync(DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<RestoredMenuItem>>([]);
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
        public Guid? VenueId { get; private set; }
        public int VenueContentCount { get; private set; }
        public int ScreenContentCount { get; private set; }
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) { ScreenContentCount++; return Task.CompletedTask; }
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) { VenueId = venueId; VenueContentCount++; return Task.CompletedTask; }
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class MenuItemManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesFieldsAppendsAndNotifiesVenue()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = [new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = sectionId, Name = "Soup", SortOrder = 2 }]
        };
        var notifier = new RecordingNotifier();
        var service = new MenuItemManagementService(repository, notifier, new FixedTimeProvider());

        var created = await service.CreateAsync(venueId, menuId, sectionId, "  Burger  ", "  House special  ", 12.345m, null);

        Assert.Equal("Burger", created.Name);
        Assert.Equal("House special", created.Description);
        Assert.Equal(12.35m, created.Price);
        Assert.Equal(3, created.SortOrder);
        Assert.Equal(created, repository.CreatedItem);
        Assert.Equal(venueId, notifier.VenueId);
        Assert.Equal(1, notifier.ContentNotificationCount);
    }

    [Fact]
    public async Task UpdateAsync_ValidatesCompleteOwnershipPath()
    {
        var venueId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = Guid.NewGuid(), VenueId = venueId, Name = "Other" }]
        };
        var notifier = new RecordingNotifier();
        var service = new MenuItemManagementService(repository, notifier, new FixedTimeProvider());

        var result = await service.UpdateAsync(
            venueId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Changed",
            null,
            1,
            null);

        Assert.Null(result);
        Assert.Null(repository.UpdatedItem);
        Assert.Equal(0, notifier.ContentNotificationCount);
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidPricesWithoutWritingOrNotifying()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }]
        };
        var notifier = new RecordingNotifier();
        var service = new MenuItemManagementService(repository, notifier, new FixedTimeProvider());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.CreateAsync(venueId, menuId, sectionId, "Burger", null, -1, null));

        Assert.Null(repository.CreatedItem);
        Assert.Equal(0, notifier.ContentNotificationCount);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 29, 22, 30, 0, TimeSpan.Zero);
    }

    private sealed class FakeMenuRepository : IMenuRepository
    {
        public IReadOnlyCollection<Menu> Menus { get; init; } = [];
        public IReadOnlyCollection<MenuSection> Sections { get; init; } = [];
        public IReadOnlyCollection<MenuItem> Items { get; init; } = [];
        public MenuItem? CreatedItem { get; private set; }
        public MenuItem? UpdatedItem { get; private set; }

        public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => Task.FromResult(menu.Id);
        public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => Task.FromResult(section.Id);
        public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default)
        {
            CreatedItem = item;
            return Task.FromResult(item.Id);
        }
        public Task<Guid> CreateTranslationAsync(MenuItemTranslation translation, CancellationToken cancellationToken = default) => Task.FromResult(translation.Id);
        public Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateItemAsync(MenuItem item, CancellationToken cancellationToken = default)
        {
            UpdatedItem = item;
            return Task.FromResult(true);
        }
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
        public int ContentNotificationCount { get; private set; }
        public Guid? VenueId { get; private set; }

        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default)
        {
            VenueId = venueId;
            ContentNotificationCount++;
            return Task.CompletedTask;
        }

        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;
using Vennu.Api.Tests.TestDoubles;

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
        var service = CreateService(repository, notifier);

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
        var service = CreateService(repository, notifier);

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
        var service = CreateService(repository, notifier);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.CreateAsync(venueId, menuId, sectionId, "Burger", null, -1, null));

        Assert.Null(repository.CreatedItem);
        Assert.Equal(0, notifier.ContentNotificationCount);
    }

    [Fact]
    public async Task UpdatePresentationAsync_NormalizesBadgesAndPublishesAvailability()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = [new MenuItem { Id = itemId, VenueId = venueId, MenuSectionId = sectionId, Name = "Burger", IsAvailable = true }]
        };
        var notifier = new RecordingNotifier();
        var service = new MenuItemManagementService(
            repository,
            notifier,
            new FakeCapabilityDecisionServices("content.item.dietary_information_manage", "schedule.promotion.automate"),
            new FixedTimeProvider());

        var result = await service.UpdatePresentationAsync(
            venueId,
            menuId,
            sectionId,
            itemId,
            false,
            4,
            [" vegan ", "Vegan", "contains nuts"],
            true);

        Assert.NotNull(result);
        Assert.False(result.IsAvailable);
        Assert.Equal(4, result.QuantityAvailable);
        Assert.Equal("vegan,contains nuts", result.Tags);
        Assert.True(result.IsPopular);
        Assert.Equal(itemId, repository.UpdatedItem?.Id);
        Assert.Equal(1, notifier.AvailabilityNotificationCount);
        Assert.Equal(1, notifier.ContentNotificationCount);
    }

    [Fact]
    public async Task UpdateAsync_BlocksHappyHourChangesWithoutEffectiveFeature()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = [new MenuItem { Id = itemId, VenueId = venueId, MenuSectionId = sectionId, Name = "Burger", Price = 10 }]
        };
        var service = new MenuItemManagementService(
            repository,
            new RecordingNotifier(),
            new FakeCapabilityDecisionServices(),
            new FixedTimeProvider());

        var error = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(venueId, menuId, sectionId, itemId, "Burger", null, 10, 8));

        Assert.Contains("Happy-hour pricing", error.Message);
        Assert.Null(repository.UpdatedItem);
    }

    [Fact]
    public async Task SetActiveAsync_ArchivesOwnedItemAndNotifies()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = [new MenuItem { Id = itemId, VenueId = venueId, MenuSectionId = sectionId, Name = "Burger", IsActive = true }]
        };
        var notifier = new RecordingNotifier();
        var service = CreateService(repository, notifier);

        var archived = await service.SetActiveAsync(venueId, menuId, sectionId, itemId, false);

        Assert.NotNull(archived);
        Assert.False(archived.IsActive);
        Assert.Equal(itemId, repository.UpdatedItem?.Id);
        Assert.Equal(1, notifier.ContentNotificationCount);
    }

    [Fact]
    public async Task ReorderAsync_RequiresEveryOwnedItemAndPreservesOrder()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = [
                new MenuItem { Id = first, VenueId = venueId, MenuSectionId = sectionId, Name = "First" },
                new MenuItem { Id = second, VenueId = venueId, MenuSectionId = sectionId, Name = "Second" }
            ]
        };
        var service = CreateService(repository, new RecordingNotifier());

        await service.ReorderAsync(venueId, menuId, sectionId, [second, first]);
        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.ReorderAsync(venueId, menuId, sectionId, [first]));

        Assert.Equal(new[] { second, first }, repository.ReorderedItemIds);
        Assert.Contains("every venue menu item", error.Message);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 29, 22, 30, 0, TimeSpan.Zero);
    }

    private static MenuItemManagementService CreateService(FakeMenuRepository repository, RecordingNotifier notifier) =>
        new(repository, notifier, new FakeCapabilityDecisionServices(
            "content.item.dietary_information_manage", "schedule.promotion.automate"), new FixedTimeProvider());

    private sealed class FakeMenuRepository : IMenuRepository
    {
        public IReadOnlyCollection<Menu> Menus { get; init; } = [];
        public IReadOnlyCollection<MenuSection> Sections { get; init; } = [];
        public IReadOnlyCollection<MenuItem> Items { get; init; } = [];
        public MenuItem? CreatedItem { get; private set; }
        public MenuItem? UpdatedItem { get; private set; }
        public IReadOnlyCollection<Guid>? ReorderedItemIds { get; private set; }

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
        public Task<bool> UpdateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<IReadOnlyCollection<RestoredMenuItem>> RestoreExpiredAvailabilityAsync(DateTime utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<RestoredMenuItem>>([]);
        public Task<int> ReorderSectionsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, DateTime updatedUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult(sectionIds.Count);
        public Task<int> ReorderItemsAsync(Guid venueId, Guid sectionId, IReadOnlyCollection<Guid> itemIds, DateTime updatedUtc, CancellationToken cancellationToken = default)
        {
            ReorderedItemIds = itemIds;
            return Task.FromResult(itemIds.Count);
        }
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
        public int AvailabilityNotificationCount { get; private set; }
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
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default)
        {
            AvailabilityNotificationCount++;
            return Task.CompletedTask;
        }
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

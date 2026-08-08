using Vennu.Api.Notifications;
using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Pos;

[Trait("Category", "Unit")]
public sealed class ToastInventorySyncServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Snapshot_ReusesVenueOwnedMappingAndIsIdempotent()
    {
        var item = new MenuItem { Id = Guid.NewGuid(), VenueId = VenueId, IsAvailable = true };
        var menus = new MenuRepositoryFake();
        var notifier = new NotifierFake();
        var service = new ToastInventorySyncService(new MappingRepositoryFake(item), menus, notifier, TimeProvider.System);
        var state = new PosInventoryItem("toast-item", false, 0, null, null);

        var first = await service.ApplySnapshotAsync(VenueId, [state]);
        var second = await service.ApplySnapshotAsync(VenueId, [state]);

        Assert.Equal(1, first.ItemsUpdated);
        Assert.Equal(0, second.ItemsUpdated);
        Assert.Equal(1, menus.UpdateCount);
        Assert.Single(notifier.Availability);
        Assert.Single(notifier.Content);
    }

    [Fact]
    public async Task Snapshot_WrongVenueCannotResolveMapping()
    {
        var item = new MenuItem { Id = Guid.NewGuid(), VenueId = VenueId, IsAvailable = true };
        var menus = new MenuRepositoryFake();
        var service = new ToastInventorySyncService(new MappingRepositoryFake(item), menus, new NotifierFake(), TimeProvider.System);

        var result = await service.ApplySnapshotAsync(Guid.NewGuid(), [new PosInventoryItem("toast-item", false, 0, null, null)]);

        Assert.Equal(0, result.ItemsUpdated);
        Assert.True(item.IsAvailable);
    }

    private sealed class MappingRepositoryFake(MenuItem item) : IPosCatalogMappingRepository
    {
        public Task<MenuItem?> GetMappedItemAsync(Guid venueId, PosProvider provider, string externalItemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<MenuItem?>(venueId == item.VenueId && provider == PosProvider.Toast && externalItemId == "toast-item" ? item : null);
        public Task<IReadOnlyCollection<PosCatalogMapping>> GetAllAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PosCatalogMapping> SaveAsync(Guid venueId, PosCatalogMapping mapping, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class MenuRepositoryFake : IMenuRepository
    {
        public int UpdateCount { get; private set; }
        public Task<bool> UpdateItemAsync(MenuItem item, CancellationToken cancellationToken = default) { UpdateCount++; return Task.FromResult(true); }
        public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> ReorderSectionsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, DateTime updatedUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<Menu>> GetMenusAsync(Guid venueId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<MenuSection>> GetSectionsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<MenuItem>> GetItemsAsync(Guid venueId, Guid sectionId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class NotifierFake : IScreenUpdateNotifier
    {
        public List<(string, bool)> Availability { get; } = [];
        public List<object> Content { get; } = [];
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) { Availability.Add((itemId, available)); return Task.CompletedTask; }
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) { Content.Add(payload); return Task.CompletedTask; }
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long version, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long version, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

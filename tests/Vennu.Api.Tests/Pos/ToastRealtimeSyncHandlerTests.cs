using Vennu.Api.Notifications;
using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Pos;

[Trait("Category", "Unit")]
public sealed class ToastRealtimeSyncHandlerTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task LowQuantity_UpdatesMappedToastItemAndNotifiesOnce()
    {
        var item = new MenuItem { Id = ItemId, VenueId = VenueId, IsAvailable = false, QuantityAvailable = 0 };
        var menus = new MenuRepositoryFake();
        var notifier = new NotifierFake();
        var handler = Create(item, menus, notifier);
        var webhook = Event("low_quantity", """{"details":{"itemGuid":"item-1","quantity":3}}""");

        await handler.HandleAsync(webhook);
        await handler.HandleAsync(webhook);

        Assert.True(item.IsAvailable);
        Assert.Equal(3, item.QuantityAvailable);
        Assert.Equal(1, menus.UpdateCount);
        Assert.Single(notifier.Availability);
        Assert.Single(notifier.Content);
    }

    [Fact]
    public async Task UnknownRestaurant_CannotMutateMappedItem()
    {
        var item = new MenuItem { Id = ItemId, VenueId = VenueId, IsAvailable = true };
        var menus = new MenuRepositoryFake();
        var notifier = new NotifierFake();
        var handler = Create(item, menus, notifier, false);

        await handler.HandleAsync(Event("out_of_stock", """{"details":{"itemGuid":"item-1"}}"""));

        Assert.True(item.IsAvailable);
        Assert.Equal(0, menus.UpdateCount);
    }

    private static ToastRealtimeSyncHandler Create(MenuItem item, MenuRepositoryFake menus, NotifierFake notifier, bool connected = true) =>
        new(
            new ConnectionRepositoryFake(connected),
            new ImportFake(),
            notifier,
            new ToastInventorySyncService(new MappingRepositoryFake(item), menus, notifier, TimeProvider.System));

    private static PosWebhookEvent Event(string type, string payload) => new()
    {
        Provider = PosProvider.Toast,
        ProviderEventId = Guid.NewGuid().ToString(),
        EventType = type,
        ExternalMerchantId = "3325cc58-dc6e-4e21-85f9-7de275ffe820",
        Payload = payload
    };

    private sealed class ConnectionRepositoryFake(bool connected) : IPosConnectionRepository
    {
        public Task<PosConnection?> GetByExternalMerchantIdAsync(PosProvider provider, string externalMerchantId, CancellationToken cancellationToken = default) =>
            Task.FromResult<PosConnection?>(connected ? new PosConnection { VenueId = VenueId, Provider = PosProvider.Toast, Status = PosConnectionStatus.Connected, ExternalMerchantId = externalMerchantId } : null);
        public Task<PosConnection?> GetAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PosConnection> SaveAsync(Guid venueId, PosConnection connection, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class MappingRepositoryFake(MenuItem item) : IPosCatalogMappingRepository
    {
        public Task<MenuItem?> GetMappedItemAsync(Guid venueId, PosProvider provider, string externalItemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<MenuItem?>(venueId == item.VenueId && provider == PosProvider.Toast && externalItemId == "item-1" ? item : null);
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

    private sealed class ImportFake : IPosCatalogImportService
    {
        public Task<PosCatalogImportResult> ImportAsync(Guid venueId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PosCatalogImportResult> ImportAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PosCatalogImportResult("completed", 0, 0, 0, 0, 0, [], DateTime.UtcNow));
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

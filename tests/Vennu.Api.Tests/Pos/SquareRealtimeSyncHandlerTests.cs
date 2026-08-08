using Vennu.Api.Notifications;
using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Pos;

[Trait("Category", "Unit")]
public sealed class SquareRealtimeSyncHandlerTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 1, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task InventoryEvent_ChangesAvailabilityOnceAndUsesAvailabilityNotification()
    {
        var item = Item();
        var menus = new MenuRepositoryFake();
        var notifier = new NotifierFake();
        var handler = Create(item, menus, notifier);
        var payload = """{"data":{"object":{"inventory_counts":[{"catalog_object_id":"variation-1","state":"IN_STOCK","quantity":"0"}]}}}""";
        var webhook = Event("inventory.count.updated", payload);

        await handler.HandleAsync(webhook);
        await handler.HandleAsync(webhook);

        Assert.False(item.IsAvailable);
        Assert.Equal(0, item.QuantityAvailable);
        Assert.Equal(1, menus.UpdateCount);
        Assert.Equal((ItemId.ToString(), false), Assert.Single(notifier.Availability));
        Assert.Single(notifier.Content);
    }

    [Fact]
    public async Task CatalogEvent_ChangesMappedUsdPriceOnceAndUsesContentNotification()
    {
        var item = Item();
        var menus = new MenuRepositoryFake();
        var notifier = new NotifierFake();
        var provider = new ProviderFake(new PosCatalogResult([], [new PosCatalogItem("variation-1", "category-1", "Burger", null, 14.25m, "USD", [])]));
        var handler = Create(item, menus, notifier, provider);
        var webhook = Event("catalog.version.updated", "{}");

        await handler.HandleAsync(webhook);
        await handler.HandleAsync(webhook);

        Assert.Equal(14.25m, item.Price);
        Assert.Equal(1, menus.UpdateCount);
        Assert.Single(notifier.Content);
        Assert.Empty(notifier.Availability);
    }

    [Fact]
    public async Task UnknownMerchant_CannotMutateMappedItem()
    {
        var item = Item();
        var menus = new MenuRepositoryFake();
        var notifier = new NotifierFake();
        var handler = Create(item, menus, notifier, hasConnection: false);

        await handler.HandleAsync(Event("inventory.count.updated", """{"data":{"object":{"inventory_counts":[{"catalog_object_id":"variation-1","state":"IN_STOCK","quantity":"0"}]}}}"""));

        Assert.True(item.IsAvailable);
        Assert.Equal(0, menus.UpdateCount);
        Assert.Empty(notifier.Availability);
    }

    private static SquareRealtimeSyncHandler Create(
        MenuItem item,
        MenuRepositoryFake menus,
        NotifierFake notifier,
        IPosProvider? provider = null,
        bool hasConnection = true) =>
        new(
            new ConnectionRepositoryFake(hasConnection ? Connected() : null),
            new MappingRepositoryFake(item),
            menus,
            [provider ?? new ProviderFake(new PosCatalogResult([], []))],
            new ProtectorFake(),
            notifier,
            new FixedTimeProvider(UtcNow));

    private static PosWebhookEvent Event(string type, string payload) => new()
    {
        Provider = PosProvider.Square,
        ProviderEventId = Guid.NewGuid().ToString(),
        EventType = type,
        ExternalMerchantId = "merchant-1",
        Payload = payload
    };

    private static MenuItem Item() => new() { Id = ItemId, VenueId = VenueId, Price = 12m, IsAvailable = true };
    private static PosConnection Connected() => new() { VenueId = VenueId, Provider = PosProvider.Square, Status = PosConnectionStatus.Connected, ExternalMerchantId = "merchant-1", ProtectedAccessToken = "protected:access" };

    private sealed class ConnectionRepositoryFake(PosConnection? connection) : IPosConnectionRepository
    {
        public Task<PosConnection?> GetByExternalMerchantIdAsync(PosProvider provider, string externalMerchantId, CancellationToken cancellationToken = default) =>
            Task.FromResult(connection is not null && connection.Provider == provider && connection.ExternalMerchantId == externalMerchantId ? connection : null);
        public Task<PosConnection?> GetAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PosConnection> SaveAsync(Guid venueId, PosConnection value, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class MappingRepositoryFake(MenuItem item) : IPosCatalogMappingRepository
    {
        public Task<MenuItem?> GetMappedItemAsync(Guid venueId, PosProvider provider, string externalItemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<MenuItem?>(venueId == item.VenueId && provider == PosProvider.Square && externalItemId == "variation-1" ? item : null);
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

    private sealed class ProviderFake(PosCatalogResult catalog) : IPosProvider
    {
        public PosProvider Provider => PosProvider.Square;
        public Task<PosCatalogResult> GetCatalogAsync(PosProviderContext context, CancellationToken cancellationToken = default) => Task.FromResult(catalog);
        public Task<PosInventoryResult> GetInventoryAsync(PosProviderContext context, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class ProtectorFake : IPosCredentialProtector
    {
        public string Protect(string plaintext) => throw new NotSupportedException();
        public string Unprotect(string protectedValue) => protectedValue["protected:".Length..];
    }

    private sealed class NotifierFake : IScreenUpdateNotifier
    {
        public List<(string ItemId, bool Available)> Availability { get; } = [];
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

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider { public override DateTimeOffset GetUtcNow() => value; }
}

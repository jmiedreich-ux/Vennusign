using Vennu.Api.Notifications;
using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Pos;

[Trait("Category", "Unit")]
public sealed class CloverRealtimeSyncHandlerTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 3, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Update_AppliesMappedAvailabilityQuantityAndUsdPriceIdempotently()
    {
        var item = new MenuItem { Id = ItemId, VenueId = VenueId, IsAvailable = true, Price = 12m };
        var menus = new MenuRepositoryFake();
        var notifier = new NotifierFake();
        var connections = new ConnectionRepositoryFake(Connected());
        var handler = Create(item, menus, notifier, connections, new ProviderFake(
            new PosInventoryItem("item-1", false, 0, 14.25m, "USD")));
        var webhook = Event("inventory.item.update");

        await handler.HandleAsync(webhook);
        await handler.HandleAsync(webhook);

        Assert.False(item.IsAvailable);
        Assert.Equal(0, item.QuantityAvailable);
        Assert.Equal(14.25m, item.Price);
        Assert.Equal(1, menus.UpdateCount);
        Assert.Single(notifier.Availability);
        Assert.Single(notifier.Content);
        Assert.Equal(UtcNow.UtcDateTime, connections.Connection!.LastSyncedUtc);
    }

    [Fact]
    public async Task UnknownMerchant_CannotFetchOrMutateItem()
    {
        var item = new MenuItem { Id = ItemId, VenueId = VenueId, IsAvailable = true, Price = 12m };
        var menus = new MenuRepositoryFake();
        var provider = new ProviderFake(new PosInventoryItem("item-1", false, 0, 14.25m, "USD"));
        var handler = Create(item, menus, new NotifierFake(), new ConnectionRepositoryFake(null), provider);

        await handler.HandleAsync(Event("inventory.item.update"));

        Assert.Equal(0, provider.CallCount);
        Assert.Equal(0, menus.UpdateCount);
        Assert.True(item.IsAvailable);
    }

    private static CloverRealtimeSyncHandler Create(
        MenuItem item,
        MenuRepositoryFake menus,
        NotifierFake notifier,
        ConnectionRepositoryFake connections,
        ProviderFake provider) =>
        new(connections, new MappingRepositoryFake(item), menus, [provider], new ProtectorFake(), notifier, new FixedTimeProvider(UtcNow));

    private static PosWebhookEvent Event(string type) => new()
    {
        Provider = PosProvider.Clover,
        ProviderEventId = Guid.NewGuid().ToString(),
        EventType = type,
        ExternalMerchantId = "merchant-1",
        Payload = """{ "objectId": "I:item-1", "type": "UPDATE", "ts": 1785556800000 }"""
    };

    private static PosConnection Connected() => new()
    {
        VenueId = VenueId,
        Provider = PosProvider.Clover,
        Status = PosConnectionStatus.Connected,
        ExternalMerchantId = "merchant-1",
        ProtectedAccessToken = "protected:access"
    };

    private sealed class ConnectionRepositoryFake(PosConnection? connection) : IPosConnectionRepository
    {
        public PosConnection? Connection => connection;
        public Task<PosConnection?> GetByExternalMerchantIdAsync(PosProvider provider, string externalMerchantId, CancellationToken cancellationToken = default) =>
            Task.FromResult(connection is not null && connection.Provider == provider && connection.ExternalMerchantId == externalMerchantId ? connection : null);
        public Task<PosConnection> SaveAsync(Guid venueId, PosConnection value, CancellationToken cancellationToken = default) => Task.FromResult(value);
        public Task<PosConnection?> GetAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class MappingRepositoryFake(MenuItem item) : IPosCatalogMappingRepository
    {
        public Task<MenuItem?> GetMappedItemAsync(Guid venueId, PosProvider provider, string externalItemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<MenuItem?>(venueId == item.VenueId && provider == PosProvider.Clover && externalItemId == "item-1" ? item : null);
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
        public Task<Guid> CreateTranslationAsync(MenuItemTranslation translation, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<RestoredMenuItem>> RestoreExpiredAvailabilityAsync(DateTime utcNow, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> ReorderSectionsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, DateTime updatedUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<Menu>> GetMenusAsync(Guid venueId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<MenuSection>> GetSectionsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<MenuItem>> GetItemsAsync(Guid venueId, Guid sectionId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<MenuItemTranslation>> GetTranslationsAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class ProviderFake(PosInventoryItem item) : IPosProvider
    {
        public int CallCount { get; private set; }
        public PosProvider Provider => PosProvider.Clover;
        public Task<PosCatalogResult> GetCatalogAsync(PosProviderContext context, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PosInventoryResult> GetInventoryAsync(PosProviderContext context, CancellationToken cancellationToken = default)
        {
            CallCount++;
            Assert.Equal("merchant-1", context.ExternalMerchantId);
            Assert.Equal(["item-1"], context.InventoryExternalItemIds);
            return Task.FromResult(new PosInventoryResult([item], UtcNow));
        }
    }

    private sealed class ProtectorFake : IPosCredentialProtector
    {
        public string Protect(string plaintext) => throw new NotSupportedException();
        public string Unprotect(string protectedValue) => protectedValue["protected:".Length..];
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

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider { public override DateTimeOffset GetUtcNow() => value; }
}

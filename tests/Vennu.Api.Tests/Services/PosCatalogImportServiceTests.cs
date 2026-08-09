using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class PosCatalogImportServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 0, 50, 0, TimeSpan.Zero);

    [Fact]
    public async Task ImportAsync_RepeatedCatalog_UpdatesMappedEntitiesWithoutDuplicates()
    {
        var provider = new ProviderFake(new PosCatalogResult(
            [new PosCatalogCategory("category-1", "Food", 0)],
            [new PosCatalogItem("variation-1", "category-1", "Burger", "Beef", 12.50m, "USD",
                [new PosCatalogModifier("modifier-1", "Cheese", 1m)])]));
        var connections = new ConnectionRepositoryFake();
        var mappings = new MappingRepositoryFake();
        var menus = new MenuRepositoryFake();
        var service = Create(provider, connections, mappings, menus);

        var first = await service.ImportAsync(VenueId);
        var second = await service.ImportAsync(VenueId);

        Assert.Equal("completed", first.Status);
        Assert.Equal(1, first.CategoriesCreated);
        Assert.Equal(1, first.ItemsCreated);
        Assert.Equal(0, second.CategoriesCreated);
        Assert.Equal(1, second.CategoriesUpdated);
        Assert.Equal(0, second.ItemsCreated);
        Assert.Equal(1, second.ItemsUpdated);
        Assert.Single(menus.Menus);
        Assert.Single(menus.Sections);
        Assert.Single(menus.Items);
        Assert.Equal("Cheese", menus.Items[0].Tags);
        Assert.Equal(4, mappings.Values.Count);
        Assert.Equal(UtcNow.UtcDateTime, Assert.IsType<PosConnection>(connections.Connection).LastSyncedUtc);
    }

    [Fact]
    public async Task ImportAsync_ReportsUnknownCategoryWithoutCreatingItem()
    {
        var provider = new ProviderFake(new PosCatalogResult(
            [new PosCatalogCategory("category-1", "Food", 0)],
            [new PosCatalogItem("variation-1", "missing", "Burger", null, 12m, "USD", [])]));
        var menus = new MenuRepositoryFake();
        var service = Create(provider, new ConnectionRepositoryFake(), new MappingRepositoryFake(), menus);

        var result = await service.ImportAsync(VenueId);

        Assert.Equal("completed_with_conflicts", result.Status);
        Assert.Contains(result.Conflicts, value => value.Contains("unavailable category missing", StringComparison.Ordinal));
        Assert.Empty(menus.Items);
    }

    [Fact]
    public async Task ImportAsync_RequiresConnectedVenue()
    {
        var connections = new ConnectionRepositoryFake { Connection = null };
        var service = Create(new ProviderFake(new PosCatalogResult([], [])), connections, new MappingRepositoryFake(), new MenuRepositoryFake());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ImportAsync(VenueId));

        Assert.Equal("Connect Square before importing its catalog.", exception.Message);
    }

    private static PosCatalogImportService Create(
        IPosProvider provider,
        IPosConnectionRepository connections,
        IPosCatalogMappingRepository mappings,
        IMenuRepository menus) =>
        new([provider], connections, mappings, menus, new ProtectorFake(), new FixedTimeProvider(UtcNow));

    private sealed class ProviderFake(PosCatalogResult catalog) : IPosProvider
    {
        public PosProvider Provider => PosProvider.Square;
        public Task<PosCatalogResult> GetCatalogAsync(PosProviderContext context, CancellationToken cancellationToken = default) => Task.FromResult(catalog);
        public Task<PosInventoryResult> GetInventoryAsync(PosProviderContext context, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class ConnectionRepositoryFake : IPosConnectionRepository
    {
        public PosConnection? Connection { get; set; } = new()
        {
            Id = Guid.NewGuid(), VenueId = VenueId, Provider = PosProvider.Square,
            Status = PosConnectionStatus.Connected, ExternalMerchantId = "merchant-1",
            ProtectedAccessToken = "protected:access"
        };
        public Task<PosConnection?> GetAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult(Connection);
        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PosConnection>>(Connection is null ? [] : [Connection]);
        public Task<PosConnection> SaveAsync(Guid venueId, PosConnection connection, CancellationToken cancellationToken = default) { Connection = connection; return Task.FromResult(connection); }
        public Task<bool> DeleteAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class MappingRepositoryFake : IPosCatalogMappingRepository
    {
        public List<PosCatalogMapping> Values { get; } = [];
        public Task<IReadOnlyCollection<PosCatalogMapping>> GetAllAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<PosCatalogMapping>>(Values.Where(value => value.VenueId == venueId && value.Provider == provider).ToArray());
        public Task<PosCatalogMapping> SaveAsync(Guid venueId, PosCatalogMapping mapping, CancellationToken cancellationToken = default)
        {
            var existing = Values.SingleOrDefault(value => value.VenueId == venueId && value.Provider == mapping.Provider && value.EntityType == mapping.EntityType && value.ExternalId == mapping.ExternalId);
            if (existing is not null) { existing.LocalEntityId = mapping.LocalEntityId; return Task.FromResult(existing); }
            mapping.Id = Guid.NewGuid(); Values.Add(mapping); return Task.FromResult(mapping);
        }
    }

    private sealed class MenuRepositoryFake : IMenuRepository
    {
        public List<Menu> Menus { get; } = [];
        public List<MenuSection> Sections { get; } = [];
        public List<MenuItem> Items { get; } = [];
        public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) { menu.Id = Guid.NewGuid(); Menus.Add(menu); return Task.FromResult(menu.Id); }
        public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) { section.Id = Guid.NewGuid(); Sections.Add(section); return Task.FromResult(section.Id); }
        public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default) { item.Id = Guid.NewGuid(); Items.Add(item); return Task.FromResult(item.Id); }
        public Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<int> ReorderSectionsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, DateTime updatedUtc, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<IReadOnlyCollection<Menu>> GetMenusAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Menu>>(Menus.Where(value => value.VenueId == venueId).ToArray());
        public Task<IReadOnlyCollection<MenuSection>> GetSectionsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<MenuSection>>(Sections.Where(value => value.VenueId == venueId && value.MenuId == menuId).ToArray());
        public Task<IReadOnlyCollection<MenuItem>> GetItemsAsync(Guid venueId, Guid sectionId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<MenuItem>>(Items.Where(value => value.VenueId == venueId && value.MenuSectionId == sectionId).ToArray());
    }

    private sealed class ProtectorFake : IPosCredentialProtector
    {
        public string Protect(string plaintext) => $"protected:{plaintext}";
        public string Unprotect(string protectedValue) => protectedValue["protected:".Length..];
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

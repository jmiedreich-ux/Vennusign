using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class MenuSectionManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_TrimsNameAndAppendsSection()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main" }],
            Sections = [new MenuSection { Id = Guid.NewGuid(), VenueId = venueId, MenuId = menuId, Name = "Food", SortOrder = 0 }]
        };
        var service = new MenuSectionManagementService(repository, new FakeFeatureResolutionService(), new FixedTimeProvider());

        var created = await service.CreateAsync(venueId, menuId, "  Drinks  ");

        Assert.Equal("Drinks", created.Name);
        Assert.Equal(1, created.SortOrder);
        Assert.Equal(created, repository.CreatedSection);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotCrossVenueBoundary()
    {
        var repository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Name = "Other" }]
        };
        var service = new MenuSectionManagementService(repository, new FakeFeatureResolutionService(), new FixedTimeProvider());

        var result = await service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), "Changed", false);

        Assert.Null(result);
        Assert.Null(repository.UpdatedSection);
    }

    [Fact]
    public async Task ReorderAsync_RequiresEverySectionExactlyOnce()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }]
        };
        var service = new MenuSectionManagementService(repository, new FakeFeatureResolutionService(), new FixedTimeProvider());

        var error = await Assert.ThrowsAsync<ArgumentException>(
            () => service.ReorderAsync(venueId, menuId, [Guid.NewGuid()]));

        Assert.Contains("every venue menu section", error.Message);
    }

    [Fact]
    public async Task ReorderAsync_PreservesRequestedOrder()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var repository = new FakeMenuRepository
        {
            Sections =
            [
                new MenuSection { Id = first, VenueId = venueId, MenuId = menuId, Name = "First" },
                new MenuSection { Id = second, VenueId = venueId, MenuId = menuId, Name = "Second" }
            ]
        };
        var service = new MenuSectionManagementService(repository, new FakeFeatureResolutionService(), new FixedTimeProvider());

        var changed = await service.ReorderAsync(venueId, menuId, [second, first]);

        Assert.Equal(2, changed);
        Assert.Equal(new[] { second, first }, repository.ReorderedSectionIds);
    }

    [Fact]
    public async Task GetAsync_UsesEffectiveFeatureResolutionForCapabilities()
    {
        var service = new MenuSectionManagementService(
            new FakeMenuRepository(),
            new FakeFeatureResolutionService("happy_hour"),
            new FixedTimeProvider());

        var snapshot = await service.GetAsync(Guid.NewGuid());

        Assert.True(snapshot.Capabilities.HappyHour);
        Assert.False(snapshot.Capabilities.AllergenBadges);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 29, 22, 20, 0, TimeSpan.Zero);
    }

    private sealed class FakeFeatureResolutionService(params string[] enabledFeatures) : IFeatureResolutionService
    {
        private readonly HashSet<string> enabled = new(enabledFeatures, StringComparer.OrdinalIgnoreCase);

        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult(enabled.Contains(featureKey));
        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<FeatureEntitlement?>(enabled.Contains(featureKey) ? new(featureKey, true, null, "test") : null);
        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(enabled.ToDictionary(key => key, key => new FeatureEntitlement(key, true, null, "test")));
        public void Invalidate(Guid venueId) { }
    }

    private sealed class FakeMenuRepository : IMenuRepository
    {
        public IReadOnlyCollection<Menu> Menus { get; init; } = [];
        public IReadOnlyCollection<MenuSection> Sections { get; init; } = [];
        public MenuSection? CreatedSection { get; private set; }
        public MenuSection? UpdatedSection { get; private set; }
        public IReadOnlyCollection<Guid>? ReorderedSectionIds { get; private set; }

        public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) => Task.FromResult(menu.Id);
        public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default)
        {
            CreatedSection = section;
            return Task.FromResult(section.Id);
        }
        public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => Task.FromResult(item.Id);
        public Task<Guid> CreateTranslationAsync(MenuItemTranslation translation, CancellationToken cancellationToken = default) => Task.FromResult(translation.Id);
        public Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default)
        {
            UpdatedSection = section;
            return Task.FromResult(true);
        }
        public Task<bool> UpdateItemAsync(MenuItem item, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<int> ReorderSectionsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, DateTime updatedUtc, CancellationToken cancellationToken = default)
        {
            ReorderedSectionIds = sectionIds;
            return Task.FromResult(sectionIds.Count);
        }
        public Task<IReadOnlyCollection<Menu>> GetMenusAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Menu>>(Menus.Where(menu => menu.VenueId == venueId).ToArray());
        public Task<IReadOnlyCollection<MenuSection>> GetSectionsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<MenuSection>>(Sections.Where(section => section.VenueId == venueId && section.MenuId == menuId).ToArray());
        public Task<IReadOnlyCollection<MenuItem>> GetItemsAsync(Guid venueId, Guid sectionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<MenuItem>>([]);
        public Task<IReadOnlyCollection<MenuItemTranslation>> GetTranslationsAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<MenuItemTranslation>>([]);
    }
}

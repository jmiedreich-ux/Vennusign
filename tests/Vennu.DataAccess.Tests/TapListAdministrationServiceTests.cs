using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class TapListAdministrationServiceTests
{
    [Fact]
    public async Task CreateItemAsync_NormalizesFieldsAndRequiresVenueCategory()
    {
        var venueId = Guid.NewGuid();
        var category = new TapCategory { Id = Guid.NewGuid(), VenueId = venueId, Name = "Draft", SortOrder = 0 };
        var repository = new FakeTapListRepository { Categories = [category] };
        var service = new TapListAdministrationService(repository, new FakeVenueRepository(venueId), TimeProvider.System);

        var created = await service.CreateItemAsync(venueId, new TapItem
        {
            TapCategoryId = category.Id, Name = "  480B  ", Style = "  West Coast IPA ",
            Price = 7m, Abv = 8.2m, Ibu = 65, GlassColor = "#f5c842", NameColor = "#ffd700",
            IsAvailable = true
        });

        Assert.Equal("480B", created.Name);
        Assert.Equal("West Coast IPA", created.Style);
        Assert.Equal("#F5C842", created.GlassColor);
        Assert.Same(created, repository.Items.Single());
    }

    [Fact]
    public async Task CreateItemAsync_RejectsCategoryFromAnotherVenue()
    {
        var venueId = Guid.NewGuid();
        var service = new TapListAdministrationService(
            new FakeTapListRepository(),
            new FakeVenueRepository(venueId),
            TimeProvider.System);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateItemAsync(
            venueId,
            new TapItem { TapCategoryId = Guid.NewGuid(), Name = "Beer", Price = 5m }));

        Assert.Equal("categoryId", exception.ParamName);
    }

    [Fact]
    public async Task ReorderItemsAsync_RequiresExactVenueSet()
    {
        var venueId = Guid.NewGuid();
        var first = new TapItem { Id = Guid.NewGuid(), VenueId = venueId };
        var second = new TapItem { Id = Guid.NewGuid(), VenueId = venueId };
        var repository = new FakeTapListRepository { Items = [first, second] };
        var service = new TapListAdministrationService(repository, new FakeVenueRepository(venueId), TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ReorderItemsAsync(venueId, [first.Id]));
        await service.ReorderItemsAsync(venueId, [second.Id, first.Id]);

        Assert.Equal([second.Id, first.Id], repository.LastItemOrder);
    }

    private sealed class FakeTapListRepository : ITapListRepository
    {
        public List<TapCategory> Categories { get; set; } = [];
        public List<TapItem> Items { get; set; } = [];
        public IReadOnlyCollection<Guid> LastItemOrder { get; private set; } = [];
        public Task<IReadOnlyCollection<TapCategory>> GetCategoriesAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TapCategory>>(Categories);
        public Task<IReadOnlyCollection<TapItem>> GetItemsAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TapItem>>(Items);
        public Task<Guid> CreateCategoryAsync(TapCategory value, CancellationToken cancellationToken = default) { Categories.Add(value); return Task.FromResult(value.Id); }
        public Task<Guid> CreateItemAsync(TapItem value, CancellationToken cancellationToken = default) { Items.Add(value); return Task.FromResult(value.Id); }
        public Task<bool> UpdateCategoryAsync(TapCategory value, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateItemAsync(TapItem value, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteCategoryAsync(Guid venueId, Guid categoryId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<int> ReorderCategoriesAsync(Guid venueId, IReadOnlyCollection<Guid> ids, DateTime updatedUtc, CancellationToken cancellationToken = default) => Task.FromResult(ids.Count);
        public Task<int> ReorderItemsAsync(Guid venueId, IReadOnlyCollection<Guid> ids, DateTime updatedUtc, CancellationToken cancellationToken = default) { LastItemOrder = ids; return Task.FromResult(ids.Count); }
    }

    private sealed class FakeVenueRepository(Guid venueId) : IVenueRepository
    {
        public Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Venue>>([]);
        public Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Venue?>(id == venueId ? new Venue { Id = venueId, Name = "Taproom" } : null);
    }
}

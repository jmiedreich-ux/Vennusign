using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Api.Tests.TestDoubles;

namespace Vennu.Api.Tests.Services;

/// <summary>
/// The legacy editor's item surface, consolidated onto the library: a create is
/// an Item plus a Placement, an update is venue-scoped, and everything an edit
/// does here is part of the working state the derived draft compares.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenuItemManagementServiceTests
{
    private static readonly Guid VenueId = Guid.NewGuid();
    private static readonly Guid MenuId = Guid.NewGuid();
    private static readonly Guid SectionId = Guid.NewGuid();

    private static FakeMenuLibraryRepository Repository()
    {
        var repository = new FakeMenuLibraryRepository();
        repository.Sections.Add(new MenuSection { Id = SectionId, VenueId = VenueId, MenuId = MenuId, Name = "Food", IsActive = true });
        return repository;
    }

    private static MenuItemManagementService Service(FakeMenuLibraryRepository repository, RecordingNotifier notifier) =>
        new(repository, notifier, new FixedTimeProvider());

    [Fact]
    public async Task CreateAsync_WritesTheLibraryItemAndItsPlacementTogether()
    {
        var repository = Repository();
        var notifier = new RecordingNotifier();
        var service = Service(repository, notifier);

        var created = await service.CreateAsync(VenueId, MenuId, SectionId, "  Burger  ", "  House special  ", 12.345m);

        Assert.Equal("Burger", created.Name);
        Assert.Equal("House special", created.Description);
        Assert.Equal(12.35m, created.Price);
        var item = Assert.Single(repository.Items);
        // Stored exactly as it will render (Q115/Q190).
        Assert.Equal("12.35", item.Price);
        var placement = Assert.Single(repository.Placements);
        Assert.Equal(item.Id, placement.ItemId);
        Assert.Equal(SectionId, placement.MenuSectionId);
        Assert.Equal(1, notifier.ContentNotificationCount);
    }

    [Fact]
    public async Task CreateAsync_RefusesASectionThatIsNotOnThisMenu()
    {
        var repository = Repository();
        var notifier = new RecordingNotifier();
        var service = Service(repository, notifier);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(VenueId, MenuId, Guid.NewGuid(), "Burger", null, 5m));

        Assert.Empty(repository.Items);
        Assert.Equal(0, notifier.ContentNotificationCount);
    }

    // Q201: the refusal is a plain sentence and nothing is written.
    [Fact]
    public async Task CreateAsync_RefusesInPlainWordsAtTheItemsCeiling()
    {
        var repository = Repository();
        repository.Ceilings[MenuCeilings.ItemsPerMenu] = 1;
        var notifier = new RecordingNotifier();
        var service = Service(repository, notifier);
        await service.CreateAsync(VenueId, MenuId, SectionId, "First", null, 1m);

        var refusal = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(VenueId, MenuId, SectionId, "Second", null, 2m));

        Assert.Contains("set up for 1", refusal.Message, StringComparison.Ordinal);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidPricesWithoutWritingOrNotifying()
    {
        var repository = Repository();
        var notifier = new RecordingNotifier();
        var service = Service(repository, notifier);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.CreateAsync(VenueId, MenuId, SectionId, "Burger", null, -1));

        Assert.Empty(repository.Items);
        Assert.Equal(0, notifier.ContentNotificationCount);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullWhenTheItemIsNotPlacedOnThatSection()
    {
        var repository = Repository();
        var notifier = new RecordingNotifier();
        var service = Service(repository, notifier);
        await service.CreateAsync(VenueId, MenuId, SectionId, "Burger", null, 5m);

        var result = await service.UpdateAsync(VenueId, MenuId, SectionId, Guid.NewGuid(), "Changed", null, 1m);

        Assert.Null(result);
        Assert.Equal("Burger", Assert.Single(repository.Items).Name);
    }

    [Fact]
    public async Task UpdateAsync_ChangesTheLibraryItemThePlacementRenders()
    {
        var repository = Repository();
        var notifier = new RecordingNotifier();
        var service = Service(repository, notifier);
        var created = await service.CreateAsync(VenueId, MenuId, SectionId, "Burger", null, 5m);

        var updated = await service.UpdateAsync(VenueId, MenuId, SectionId, created.Id, "Smash Burger", "Two patties", 9.5m);

        Assert.NotNull(updated);
        var item = Assert.Single(repository.Items);
        Assert.Equal("Smash Burger", item.Name);
        Assert.Equal("9.5", item.Price);
    }

    [Fact]
    public async Task ReorderAsync_RequiresTheCompleteSetAndRewritesSortOrders()
    {
        var repository = Repository();
        var notifier = new RecordingNotifier();
        var service = Service(repository, notifier);
        var first = await service.CreateAsync(VenueId, MenuId, SectionId, "First", null, 1m);
        var second = await service.CreateAsync(VenueId, MenuId, SectionId, "Second", null, 2m);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ReorderAsync(VenueId, MenuId, SectionId, [first.Id]));

        var changed = await service.ReorderAsync(VenueId, MenuId, SectionId, [second.Id, first.Id]);

        Assert.Equal(2, changed);
        Assert.Equal(0, repository.Placements.Single(p => p.ItemId == second.Id).SortOrder);
        Assert.Equal(1, repository.Placements.Single(p => p.ItemId == first.Id).SortOrder);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 29, 22, 30, 0, TimeSpan.Zero);
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

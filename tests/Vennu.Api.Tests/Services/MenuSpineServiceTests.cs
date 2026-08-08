using Vennu.Api.Services;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Services;

/// <summary>
/// The save model's rules, asserted against acceptance criteria 1-4:
/// availability commits instantly and never joins a draft; a publish ships that
/// menu's whole queue and nothing else; an 86 survives a publish; and nothing
/// reaches a screen without a deliberate act.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenuSpineServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MenuId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OtherMenuId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ItemId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid ScreenId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // Criterion 1: an 86 reaches every screen showing the item, without a
    // publish, and adds nothing to the draft queue.
    [Fact]
    public async Task SetAvailability_CommitsInstantly_AndNeverQueues()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        var result = await service.SetAvailabilityAsync(VenueId, ItemId, isAvailable: false, "Alex");

        Assert.False(result.Availability.IsAvailable);
        Assert.Equal("Alex", result.Availability.ChangedBy);
        Assert.Equal(ScreenId, Assert.Single(result.ScreenIds));
        Assert.Empty(await service.GetDraftAsync(VenueId, MenuId));
    }

    // Criterion 3: an item 86'd before a publish is still 86'd after it.
    [Fact]
    public async Task Availability_SurvivesAPublish()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.SetAvailabilityAsync(VenueId, ItemId, isAvailable: false, "Alex");
        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        await service.PublishAsync(VenueId, MenuId, "Alex");

        var availability = Assert.Single(await library.GetAvailabilityAsync(VenueId));
        Assert.False(availability.IsAvailable);
    }

    // Q182: the queue is the current diff, so editing one field twice is one change.
    [Fact]
    public async Task QueueChange_ReplacesTheSameField_SoTheCountIsTheCurrentDiff()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "14", "Alex");

        var change = Assert.Single(await service.GetDraftAsync(VenueId, MenuId));
        Assert.Equal("14", change.AfterValue);
    }

    // Criterion 2: a publish ships all of this menu's changes and none of another's.
    [Fact]
    public async Task Publish_ShipsOnlyThisMenusQueue()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Menu, null, "theme", "coastal", "classic-dark", "Alex");
        await service.QueueChangeAsync(VenueId, OtherMenuId, DraftTargetKinds.Menu, null, "theme", "coastal", "classic-dark", "Dana");

        var result = await service.PublishAsync(VenueId, MenuId, "Alex");

        Assert.Equal(2, result.ChangeCount);
        Assert.Empty(await service.GetDraftAsync(VenueId, MenuId));
        Assert.Single(await service.GetDraftAsync(VenueId, OtherMenuId));
    }

    [Fact]
    public async Task Publish_ReportsEveryAssignedScreenAsATarget()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        var result = await service.PublishAsync(VenueId, MenuId, "Alex");

        var target = Assert.Single(result.Targets);
        Assert.Equal(ScreenId, target.ScreenId);
    }

    // Criterion 4: nothing reaches a screen without a deliberate act. Queuing a
    // change is not one, so the published version does not move.
    [Fact]
    public async Task QueueChange_DoesNotChangeWhatTheScreensShow()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");

        Assert.Empty(await library.GetPublishHistoryAsync(VenueId, MenuId, 10));
    }

    // "Go back to" produces a draft you then publish -- never a second silent
    // path to the screens.
    [Fact]
    public async Task GoBackTo_ProducesADraftRatherThanPublishing()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        var published = await service.PublishAsync(VenueId, MenuId, "Alex");

        var draft = await service.GoBackToAsync(VenueId, MenuId, published.Event.Version, "Dana");

        Assert.NotEmpty(draft);
        // Still one publish: going back did not reach the screens by itself.
        Assert.Single(await library.GetPublishHistoryAsync(VenueId, MenuId, 10));
    }

    // The one irreversible act is recorded with its author, never anonymous.
    [Fact]
    public async Task DiscardDraft_ClearsTheQueueAndIsRecorded()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        await service.DiscardDraftAsync(VenueId, MenuId, "Dana");

        Assert.Empty(await service.GetDraftAsync(VenueId, MenuId));
        var entry = Assert.Single(await library.GetHistoryAsync(VenueId, MenuId, 10));
        Assert.Equal(MenuHistoryKinds.DraftDiscarded, entry.Kind);
        Assert.Equal("Dana", entry.Author);
    }

    [Fact]
    public async Task TakeOffScreens_ReleasesScreensAndKeepsHistory()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        var released = await service.TakeOffScreensAsync(VenueId, MenuId, "Dana");

        Assert.Equal(1, released);
        Assert.Empty(await library.GetAssignmentsAsync(VenueId));
        Assert.Contains(
            await library.GetHistoryAsync(VenueId, MenuId, 10),
            entry => entry.Kind == MenuHistoryKinds.TakenOffScreens);
    }

    // Q196: times render in the venue's local time, so the context says which.
    [Fact]
    public async Task Context_ExposesVenueTimezoneAndCeilings()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        var context = await service.GetContextAsync(VenueId);

        Assert.Equal("America/New_York", context.Timezone);
        Assert.Equal(50, context.Ceilings[MenuCeilings.MenusPerVenue]);
    }

    // Q201: a ceiling refuses with a plain sentence rather than failing quietly.
    [Fact]
    public async Task Ceiling_RefusesWithAPlainSentence()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        var refusal = await service.DescribeCeilingRefusalAsync(VenueId, MenuCeilings.ImportLines, 5000);

        Assert.NotNull(refusal);
        Assert.Contains("too big", refusal, StringComparison.Ordinal);
        Assert.Null(await service.DescribeCeilingRefusalAsync(VenueId, MenuCeilings.ImportLines, 10));
    }

    private static MenuSpineService CreateService(FakeMenuLibraryRepository library) =>
        new(
            library,
            new StubVenueRepository(new Venue { Id = VenueId, Name = "Harborview", Timezone = "America/New_York" }),
            new FixedTimeProvider());

    private static FakeMenuLibraryRepository SeededLibrary()
    {
        var library = new FakeMenuLibraryRepository();
        library.Items.Add(new Item { Id = ItemId, VenueId = VenueId, Name = "Berry Fizz", Price = 12m });
        library.Placements.Add(new Placement
        {
            Id = Guid.NewGuid(),
            VenueId = VenueId,
            MenuId = MenuId,
            MenuSectionId = Guid.NewGuid(),
            ItemId = ItemId,
            SortOrder = 0
        });
        library.Assignments.Add(new MenuScreenAssignment
        {
            Id = Guid.NewGuid(),
            VenueId = VenueId,
            ScreenId = ScreenId,
            MenuId = MenuId,
            AssignedUtc = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        library.Ceilings[MenuCeilings.MenusPerVenue] = 50;
        library.Ceilings[MenuCeilings.ItemsPerMenu] = 500;
        library.Ceilings[MenuCeilings.ImportLines] = 2000;
        library.Ceilings[MenuCeilings.HistoryRetention] = 50;
        return library;
    }

    private sealed class StubVenueRepository(Venue venue) : IVenueRepository
    {
        public Task<Guid> CreateAsync(Venue value, CancellationToken cancellationToken = default) =>
            Task.FromResult(value.Id);

        public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Venue>>([venue]);

        public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Venue?>(venue.Id == venueId ? venue : null);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 8, 8, 6, 30, 0, TimeSpan.Zero);
    }
}

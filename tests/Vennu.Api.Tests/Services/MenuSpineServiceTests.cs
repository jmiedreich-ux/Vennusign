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

    // "Go back to" rebuilds the draft from the published snapshot and REPLACES
    // whatever was queued (Q67), rather than stacking a marker on top of it.
    [Fact]
    public async Task GoBackTo_RebuildsTheDraftFromTheSnapshotAndReplacesWhatWasQueued()
    {
        var library = SeededLibrary();
        // The published version had the item priced at 12.
            library.SnapshotJson =
                "{\"menuId\":\"" + MenuId + "\",\"sections\":[{\"sectionId\":\"" + Guid.Empty +
                "\",\"items\":[{\"itemId\":\"" + ItemId + "\",\"name\":\"Berry Fizz\",\"price\":\"12\"}]}]}";
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        var published = await service.PublishAsync(VenueId, MenuId, "Alex");

        // The board has since moved on, and an unrelated edit is queued.
        library.Items.Single(i => i.Id == ItemId).Price = "14";
        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "name", "Berry Fizz", "Berry Fizzz", "Dana");

        var restore = await service.GoBackToAsync(VenueId, MenuId, published.Event.Version, "Dana");

        // The queued edit was displaced, and the caller is told how many.
        Assert.Equal(1, restore.ReplacedChangeCount);
        // The restore expresses the real difference: price back to 12.
        Assert.Contains(restore.Draft, c => c.Field == "price" && c.AfterValue == "12");
        // And it published nothing by itself.
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

    // Take-off is permanent, so unlike an 86 it queues and ships on Publish (Q68).
    [Fact]
    public async Task TakeOffScreens_QueuesRatherThanCommittingImmediately()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueTakeOffScreensAsync(VenueId, MenuId, "Dana");

        // Still on its screen until someone publishes.
        Assert.NotEmpty(await library.GetAssignmentsAsync(VenueId));
        var queued = Assert.Single(await service.GetDraftAsync(VenueId, MenuId));
        Assert.Equal(DraftTargetKinds.Screens, queued.TargetKind);
        Assert.Equal(string.Empty, queued.AfterValue);
    }

    // Publishing a menu that reaches nothing is refused as a named state (Q80).
    [Fact]
    public async Task Publish_IsRefusedWhenTheMenuIsOnNoScreen()
    {
        var library = SeededLibrary();
        library.Assignments.Clear();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");

        var refusal = await Assert.ThrowsAsync<MenuNotOnAnyScreenException>(
            () => service.PublishAsync(VenueId, MenuId, "Alex"));
        Assert.Contains("Pair a screen", refusal.Message, StringComparison.Ordinal);
        // A refused publish leaves the draft untouched.
        Assert.Single(await service.GetDraftAsync(VenueId, MenuId));
    }

    // Q182: the count is what is CURRENTLY different, so an edit taken back to the
    // published value leaves the queue entirely.
    [Fact]
    public async Task QueueChange_RemovesTheRowWhenAnEditIsTakenBackToThePublishedValue()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        Assert.Single(await service.GetDraftAsync(VenueId, MenuId));

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "12", "Alex");

        Assert.Empty(await service.GetDraftAsync(VenueId, MenuId));
    }

    // The publish reports the count it actually shipped, captured as the queue was
    // removed, not a count read beforehand.
    [Fact]
    public async Task Publish_ReportsTheCountItActuallyShipped()
    {
        var library = SeededLibrary();
        var service = CreateService(library);

        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Item, ItemId, "price", "12", "13", "Alex");
        await service.QueueChangeAsync(VenueId, MenuId, DraftTargetKinds.Menu, null, "theme", "coastal", "classic-dark", "Alex");

        var result = await service.PublishAsync(VenueId, MenuId, "Alex");

        Assert.Equal(2, result.ChangeCount);
        Assert.Equal(result.Event.ChangeCount, result.ChangeCount);
    }

    // An 86 that reports reaching a screen must actually have pushed to it.
    [Fact]
    public async Task SetAvailability_PushesToEveryScreenItClaimsToReach()
    {
        var library = SeededLibrary();
        Notifier = new RecordingNotifier();
        var service = CreateService(library);

        var result = await service.SetAvailabilityAsync(VenueId, ItemId, isAvailable: false, "Alex");

        Assert.Equal(ScreenId, Assert.Single(result.ScreenIds));
        Assert.Equal(ScreenId, Assert.Single(Notifier.ScreenAvailabilityPushes));
        Assert.Equal(1, Notifier.VenueAvailabilityPushes);
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
            Notifier,
            new FixedTimeProvider());

    private static RecordingNotifier Notifier { get; set; } = new();

    private static FakeMenuLibraryRepository SeededLibrary()
    {
        var library = new FakeMenuLibraryRepository();
        library.Items.Add(new Item { Id = ItemId, VenueId = VenueId, Name = "Berry Fizz", Price = "12" });
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

    private sealed class RecordingNotifier : Vennu.Api.Notifications.IScreenUpdateNotifier
    {
        public List<Guid> ScreenAvailabilityPushes { get; } = [];

        public int VenueAvailabilityPushes { get; private set; }

        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default)
        {
            ScreenAvailabilityPushes.Add(screenId);
            return Task.CompletedTask;
        }

        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default)
        {
            VenueAvailabilityPushes++;
            return Task.CompletedTask;
        }

        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 8, 8, 6, 30, 0, TimeSpan.Zero);
    }
}

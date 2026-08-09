using Vennu.Api.Services;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Services;

/// <summary>
/// The acts that reach a screen, and the records they leave. These run against
/// the in-memory spine, which mirrors the refusals the SQL raises, so the service
/// behaviour around them is asserted without a database.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenuSpinePublishTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MenuId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OtherMenuId = Guid.Parse("22222222-2222-2222-2222-222222222223");
    private static readonly Guid ScreenId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static string Snapshot(string price) => MenuSnapshot.Serialize(new MenuSnapshot
    {
        MenuId = MenuId,
        Name = "Summer",
        Theme = "coastal",
        DwellSeconds = 8,
        LoopWarningSeconds = 60,
        Screens = [new SnapshotScreen { ScreenId = ScreenId }],
        Sections =
        [
            new SnapshotSection
            {
                SectionId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Drinks",
                SortOrder = 0,
                Items = [new SnapshotItem { ItemId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Berry Fizz", Price = price }]
            }
        ]
    });

    private static (MenuSpineService Service, FakeMenuLibraryRepository Library) Build()
    {
        var library = new FakeMenuLibraryRepository();
        library.Assignments.Add(new MenuScreenAssignment
        {
            Id = Guid.NewGuid(),
            VenueId = VenueId,
            ScreenId = ScreenId,
            MenuId = MenuId
        });
        library.WorkingSnapshotJson = Snapshot("12");

        var service = new MenuSpineService(
            library,
            new FakeVenueRepository(),
            new RecordingNotifier(),
            TimeProvider.System);
        return (service, library);
    }

    // The defect this exists for: the shipped set was computed from one reading of
    // the menu and the snapshot from another, so history could describe content
    // that never went out. Publishing must record the diff of the snapshot it
    // actually committed (Q182).
    [Fact]
    public async Task Publish_RecordsTheDiffOfTheSnapshotItActuallyShipped()
    {
        var (service, library) = Build();
        await service.PublishAsync(VenueId, MenuId, "First");

        // An edit lands after the draft is read but before the publish commits.
        library.WorkingSnapshotJson = Snapshot("13");
        library.WorkingSnapshotAtPublish = Snapshot("14");

        var result = await service.PublishAsync(VenueId, MenuId, "Publisher");

        // The recomputed set describes the snapshot that shipped, not the one the
        // first attempt read.
        var published = MenuSnapshot.Parse(result.Event.Snapshot);
        Assert.Equal("14", Assert.Single(Assert.Single(published!.Sections!).Items!).Price);
        Assert.Contains("14", result.Event.ShippedChanges!, StringComparison.Ordinal);
        Assert.DoesNotContain("\"13\"", result.Event.ShippedChanges!, StringComparison.Ordinal);
        Assert.Equal(1, result.ChangeCount);
    }

    // Q68 and Q207: the publish that carries a take-off records it under its own
    // name, so the act is not hidden inside a generic publish entry.
    [Fact]
    public async Task PublishingATakeOff_RecordsItAsItsOwnAttributableAct()
    {
        var (service, library) = Build();
        await service.PublishAsync(VenueId, MenuId, "Publisher");

        await service.QueueTakeOffScreensAsync(VenueId, MenuId, "Chef");
        library.WorkingSnapshotJson = MenuSnapshot.Serialize(new MenuSnapshot
        {
            MenuId = MenuId,
            Name = "Summer",
            Theme = "coastal",
            DwellSeconds = 8,
            LoopWarningSeconds = 60,
            Screens = [],
            Sections = []
        });

        // Taking it off is itself attributable, at the moment the person does it.
        var takeOff = Assert.Single(library.History, entry => entry.Kind == MenuHistoryKinds.TakenOffScreens);
        Assert.Equal("Chef", takeOff.Author);

        await service.PublishAsync(VenueId, MenuId, "Publisher");

        // ...and so is the publish that carries it to the screens.
        var shipped = library.History
            .Where(entry => entry.Kind == MenuHistoryKinds.TakenOffScreens && entry.PublishEventId is not null)
            .ToArray();
        Assert.Single(shipped);
        Assert.Equal("Publisher", shipped[0].Author);
    }

    // The owner's rule: a screen another menu now owns is never touched by a stale
    // act, and never silently skipped either.
    [Fact]
    public async Task Publish_LeavesAScreenAnotherMenuNowOwnsAloneAndNamesIt()
    {
        var (service, library) = Build();
        await service.PublishAsync(VenueId, MenuId, "Publisher");

        // The menu is taken off, and the screen is given to another menu, which
        // publishes to it. Only then does the older take-off publish.
        await service.QueueTakeOffScreensAsync(VenueId, MenuId, "Chef");
        library.Assignments.Add(new MenuScreenAssignment
        {
            Id = Guid.NewGuid(),
            VenueId = VenueId,
            ScreenId = ScreenId,
            MenuId = OtherMenuId
        });

        var conflict = await Assert.ThrowsAsync<ScreensTakenByAnotherMenuException>(
            () => service.PublishAsync(VenueId, MenuId, "Publisher"));

        Assert.Contains("different menu", conflict.Message, StringComparison.Ordinal);
        // The other menu's screen is untouched: no target was created against it.
        Assert.DoesNotContain(
            library.PublishTargets,
            target => library.PublishEvents.Any(e => e.Id == target.PublishEventId && e.MenuId == MenuId)
                && target.ScreenId == ScreenId
                && library.PublishEvents.Single(e => e.Id == target.PublishEventId).Version > 1);
    }

    // Review #4: putting one back is the deliberate, ceiling-checked, attributable
    // way onto the shelf. It must be the only way, or the ceiling means nothing.
    [Fact]
    public async Task APutAwayMenu_CanBeNeitherGivenAScreenNorPublished()
    {
        var (service, library) = Build();
        await service.QueueTakeOffScreensAsync(VenueId, MenuId, "Chef");
        await service.SetPutAwayAsync(VenueId, MenuId, isPutAway: true, "Owner");

        await Assert.ThrowsAsync<MenuPutAwayException>(
            () => service.AssignAsync(VenueId, ScreenId, MenuId, "Owner"));
        await Assert.ThrowsAsync<MenuPutAwayException>(
            () => service.PublishAsync(VenueId, MenuId, "Owner"));

        // It is still put away, so it still does not count against the ceiling.
        Assert.Contains(MenuId, library.PutAwayMenus);
    }

    [Fact]
    public async Task PuttingAMenuAway_IsRefusedWhileItIsStillOnAScreen()
    {
        var (service, library) = Build();

        await Assert.ThrowsAsync<MenuStillOnScreensException>(
            () => service.SetPutAwayAsync(VenueId, MenuId, isPutAway: true, "Owner"));

        Assert.DoesNotContain(library.History, entry => entry.Kind == MenuHistoryKinds.PutAway);
    }

    [Fact]
    public async Task PuttingAMenuAway_IsAttributableAndFreesRoomUnderTheCeiling()
    {
        var (service, library) = Build();
        library.MenuCount = 2;
        library.Ceilings[MenuCeilings.MenusPerVenue] = 2;
        await service.QueueTakeOffScreensAsync(VenueId, MenuId, "Chef");

        var result = await service.SetPutAwayAsync(VenueId, MenuId, isPutAway: true, "Owner");

        Assert.True(result.Changed);
        Assert.Equal(1, result.ActiveMenuCount);
        var entry = Assert.Single(library.History, item => item.Kind == MenuHistoryKinds.PutAway);
        Assert.Equal("Owner", entry.Author);
    }

    // Putting one back is bounded by the same ceiling as creating one, or the
    // refusal's advice would just move the problem around.
    [Fact]
    public async Task PuttingAMenuBack_IsRefusedInPlainWordsWhenThereIsNoRoom()
    {
        var (service, library) = Build();
        library.MenuCount = 2;
        library.Ceilings[MenuCeilings.MenusPerVenue] = 1;
        await service.QueueTakeOffScreensAsync(VenueId, MenuId, "Chef");
        await service.SetPutAwayAsync(VenueId, MenuId, isPutAway: true, "Owner");

        var refusal = await Assert.ThrowsAsync<MenuCeilingReachedException>(
            () => service.SetPutAwayAsync(VenueId, MenuId, isPutAway: false, "Owner"));

        Assert.Contains("set up for 1", refusal.Message, StringComparison.Ordinal);
    }

    private sealed class RecordingNotifier : Vennu.Api.Notifications.IScreenUpdateNotifier
    {
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

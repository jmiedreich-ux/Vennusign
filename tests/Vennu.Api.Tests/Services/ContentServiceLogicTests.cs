using Vennu.Api.Services;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Services;

/// <summary>
/// What the service decides, and only that.
///
/// This class used to assert the content model's refusals — a put-away menu cannot be
/// published, a screen another menu owns is left alone, a ceiling refuses — against
/// an in-memory repository that re-implemented each rule in C#. Every one of those
/// assertions had an identically named twin in the SQL suite, so the unit test was
/// proving the copy rather than the product. Worse, the copy drifted: independent
/// review #6 found a defect that survived 412 green unit tests precisely because the
/// fake was wrong in the same way the author was.
///
/// So the refusals live where they are enforced, in
/// <c>tests/Vennu.Data.IntegrationTests/ContentIntegrationTests.cs</c>. What stays
/// here is the logic that has no database in it and that SQL therefore cannot reach:
/// the publish retry, and the wording of a refusal. The fake is storage plus one
/// explicit failure seam — it is told when to fail, it never decides.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ContentServiceLogicTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MenuId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ScreenId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static string Snapshot(string price) => MenuSnapshot.Serialize(new MenuSnapshot
    {
        MenuId = MenuId,
        Name = "Summer",
        Theme = null,
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

    private static (ContentService Service, FakeContentRepository Library) Build()
    {
        var library = new FakeContentRepository();
        library.Assignments.Add(new MenuScreenAssignment
        {
            Id = Guid.NewGuid(),
            VenueId = VenueId,
            ScreenId = ScreenId,
            MenuId = MenuId
        });
        library.WorkingSnapshotJson = Snapshot("12");

        var service = new ContentService(
            library,
            new FakeVenueRepository(),
            new RecordingNotifier(),
            TimeProvider.System);
        return (service, library);
    }

    [Fact]
    public async Task PlacementTransition_PassesResolvedItemsCeilingToAtomicRepositoryBoundary()
    {
        var (service, library) = Build();
        library.Ceilings[MenuCeilings.ItemsPerMenu] = 37;

        await service.TransitionPlacementAsync(
            VenueId, MenuId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), [], [Guid.NewGuid()], "Owner");

        Assert.Equal(37, library.TransitionItemsPerMenuLimit);
    }

    [Fact]
    public async Task AddNewItem_BoundsPriceToTheDomainMaximum()
    {
        var (service, library) = Build();
        var sectionId = Guid.NewGuid();
        library.Sections.Add(new MenuSection { Id = sectionId, VenueId = VenueId, MenuId = MenuId, Name = "Main" });

        await service.AddNewItemAsync(VenueId, MenuId, sectionId, Guid.NewGuid(), "Burger", "Market Price extra");

        Assert.Equal("Market Price", Assert.Single(library.Items).Price);
    }

    [Fact]
    public async Task Availability_CountsAndNotifiesEachAffectedScreenOnceAcrossMenusAndPages()
    {
        var itemId = Guid.NewGuid();
        var secondMenuId = Guid.NewGuid();
        var library = new FakeContentRepository();
        library.Items.Add(new Item { Id = itemId, VenueId = VenueId, Name = "Berry Fizz" });
        library.Placements.AddRange([
            new Placement { Id = Guid.NewGuid(), VenueId = VenueId, MenuId = MenuId, ItemId = itemId },
            new Placement { Id = Guid.NewGuid(), VenueId = VenueId, MenuId = secondMenuId, ItemId = itemId }
        ]);
        library.Assignments.AddRange([
            new MenuScreenAssignment { Id = Guid.NewGuid(), VenueId = VenueId, ScreenId = ScreenId, MenuId = MenuId, PageId = Guid.NewGuid() },
            new MenuScreenAssignment { Id = Guid.NewGuid(), VenueId = VenueId, ScreenId = ScreenId, MenuId = secondMenuId, PageId = Guid.NewGuid() }
        ]);
        var notifier = new RecordingNotifier();
        var service = new ContentService(library, new FakeVenueRepository(), notifier, TimeProvider.System);

        var result = await service.SetAvailabilityAsync(VenueId, itemId, false, "Owner");

        Assert.Equal(ScreenId, Assert.Single(result.ScreenIds));
        Assert.Equal(ScreenId, Assert.Single(notifier.AvailabilityScreenIds));
    }

    // The statement refuses a publish whose diff was computed from a menu that has
    // since moved — the SQL suite proves that. What only the service can prove is
    // what happens next: it reads the menu again and ships what it actually is now,
    // so the recorded shipped set describes the snapshot that went out (Q182).
    [Fact]
    public async Task Publish_WhenTheMenuMovedUnderneathIt_RecomputesAndShipsWhatTheMenuNowIs()
    {
        var (service, library) = Build();
        await service.PublishAsync(VenueId, MenuId, "First");

        // An edit lands between the diff and the commit, so the statement refuses.
        library.WorkingSnapshotAtPublish = Snapshot("14");
        library.FailNextPublishWith = new MenuMovedWhilePublishingException("The menu changed while it was being published.");

        var result = await service.PublishAsync(VenueId, MenuId, "Publisher");

        // The retry shipped the menu as it now is, and said so.
        var published = MenuSnapshot.Parse(result.Event.Snapshot);
        Assert.Equal("14", Assert.Single(Assert.Single(published!.Sections!).Items!).Price);
        Assert.Contains("14", result.Event.ShippedChanges!, StringComparison.Ordinal);
        Assert.Equal(1, result.ChangeCount);

        // One publish reached the screens, not two: the refused attempt committed nothing.
        Assert.Equal(2, library.PublishEvents.Count);
    }

    // The retry is bounded. A menu somebody is editing continuously must eventually
    // surface as a refusal rather than spinning, or the request never returns.
    [Fact]
    public async Task Publish_GivesUpAndSaysSoWhenTheMenuKeepsMoving()
    {
        var (service, library) = Build();
        await service.PublishAsync(VenueId, MenuId, "First");

        var attempts = 0;
        library.OnPublish = () =>
        {
            attempts++;
            throw new MenuMovedWhilePublishingException("The menu changed while it was being published.");
        };

        await Assert.ThrowsAsync<MenuMovedWhilePublishingException>(
            () => service.PublishAsync(VenueId, MenuId, "Publisher"));

        Assert.Equal(4, attempts);
    }

    // A ceiling that fails quietly is worse than one that explains itself (Q201).
    // This is a pure function, so it is asserted directly rather than through a
    // repository that would have to be talked into refusing first.
    [Theory]
    [InlineData(MenuCeilings.MenusPerVenue, 3, 2, "3 menus", "set up for 2", "Put one away first")]
    [InlineData(MenuCeilings.ItemsPerMenu, 501, 500, "501 items", "set up for 500", "Split it into two menus")]
    [InlineData(MenuCeilings.ImportLines, 2001, 2000, "2001 lines", "limit of 2000", "Split it into two menus")]
    public void ACeilingRefusal_NamesTheNumber_TheLimit_AndAWayForward(
        string capabilityId, int proposedTotal, int limit, string number, string ceiling, string wayForward)
    {
        var message = MenuCeilings.DescribeRefusal(capabilityId, proposedTotal, limit);

        Assert.Contains(number, message, StringComparison.Ordinal);
        Assert.Contains(ceiling, message, StringComparison.Ordinal);
        Assert.Contains(wayForward, message, StringComparison.Ordinal);
    }

    private sealed class RecordingNotifier : Vennu.Api.Notifications.IScreenUpdateNotifier
    {
        public List<Guid> AvailabilityScreenIds { get; } = [];
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default)
        {
            AvailabilityScreenIds.Add(screenId);
            return Task.CompletedTask;
        }
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

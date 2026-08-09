using Vennu.Core.Models;
using Vennu.Api.Services;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for the Menus spine. It deliberately mirrors the
/// invariants the SQL enforces: the Q80 refusal and the ceiling checks live
/// inside the same operations that write, and every read is venue-scoped.
/// </summary>
internal sealed class FakeMenuLibraryRepository : IMenuLibraryRepository
{
    public List<Item> Items { get; } = [];

    public List<Placement> Placements { get; } = [];

    public List<MenuSection> Sections { get; } = [];

    public List<ItemAvailability> Availability { get; } = [];

    public List<MenuScreenAssignment> Assignments { get; } = [];

    public List<MenuPublishEvent> PublishEvents { get; } = [];

    public List<MenuPublishTarget> PublishTargets { get; } = [];

    public List<MenuHistoryEntry> History { get; } = [];

    public Dictionary<string, int> Ceilings { get; } = new(StringComparer.Ordinal);

    public int MenuCount { get; set; } = 1;

    // ----- Library and placements -----

    public Task<Guid> CreateItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
        Items.Add(item);
        return Task.FromResult(item.Id);
    }

    public Task<bool> UpdateItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        var index = Items.FindIndex(candidate => candidate.Id == item.Id);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        Items[index] = item;
        return Task.FromResult(true);
    }

    public Task<Item?> GetItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(item => item.VenueId == venueId && item.Id == itemId));

    public Task<IReadOnlyCollection<Item>> GetItemsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Item>>(Items.Where(item => item.VenueId == venueId).ToArray());

    public Task<int> CountItemsOnMenuAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Placements
            .Where(placement => placement.VenueId == venueId && placement.MenuId == menuId)
            .Select(placement => placement.ItemId)
            .Distinct()
            .Count());

    public Task<Guid> CreatePlacementAsync(Placement placement, CancellationToken cancellationToken = default)
    {
        placement.Id = placement.Id == Guid.Empty ? Guid.NewGuid() : placement.Id;
        Placements.Add(placement);
        return Task.FromResult(placement.Id);
    }

    public Task<bool> RemovePlacementAsync(Guid venueId, Guid placementId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Placements.RemoveAll(placement => placement.VenueId == venueId && placement.Id == placementId) > 0);

    public Task<IReadOnlyCollection<Placement>> GetPlacementsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Placement>>(
            Placements.Where(placement => placement.VenueId == venueId && placement.MenuId == menuId).ToArray());

    public Task<IReadOnlyCollection<Placement>> GetPlacementsForItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Placement>>(
            Placements.Where(placement => placement.VenueId == venueId && placement.ItemId == itemId).ToArray());

    // ----- Availability -----

    public Task<ItemAvailability> SetAvailabilityAsync(ItemAvailability availability, CancellationToken cancellationToken = default)
    {
        Availability.RemoveAll(state => state.VenueId == availability.VenueId && state.ItemId == availability.ItemId);
        Availability.Add(availability);
        return Task.FromResult(availability);
    }

    public Task<IReadOnlyCollection<ItemAvailability>> GetAvailabilityAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ItemAvailability>>(
            Availability.Where(state => state.VenueId == venueId).ToArray());

    // ----- Assignment -----

    public Task<MenuScreenAssignment> AssignScreenAsync(MenuScreenAssignment assignment, CancellationToken cancellationToken = default)
    {
        // Giving a put-away menu a screen would put it back on the shelf without
        // the ceiling check or the record, so it is refused.
        if (PutAwayMenus.Contains(assignment.MenuId))
        {
            throw new MenuPutAwayException("This menu is put away. Put it back on the shelf before giving it a screen.");
        }

        assignment.Id = assignment.Id == Guid.Empty ? Guid.NewGuid() : assignment.Id;
        // One menu per screen: assigning replaces whatever the screen showed.
        Assignments.RemoveAll(existing => existing.ScreenId == assignment.ScreenId);
        Assignments.Add(assignment);
        return Task.FromResult(assignment);
    }

    public Task<bool> ClearScreenAssignmentAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Assignments.RemoveAll(a => a.VenueId == venueId && a.ScreenId == screenId) > 0);

    public Task<int> ClearMenuAssignmentsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Assignments.RemoveAll(a => a.VenueId == venueId && a.MenuId == menuId));

    public Task<int> TakeOffScreensAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default)
    {
        var removed = Assignments.RemoveAll(a => a.VenueId == venueId && a.MenuId == menuId);
        if (removed > 0)
        {
            History.Add(new MenuHistoryEntry
            {
                Id = Guid.NewGuid(),
                VenueId = venueId,
                MenuId = menuId,
                Kind = MenuHistoryKinds.TakenOffScreens,
                Detail = $"Took the menu off {removed} screen(s); it reaches them on the next publish.",
                Author = author,
                OccurredUtc = occurredUtc
            });
        }

        return Task.FromResult(removed);
    }

    /// <summary>Menus this fake knows are put away, so the ceiling and the act can be exercised.</summary>
    public HashSet<Guid> PutAwayMenus { get; } = [];

    /// <summary>
    /// Screens the latest published snapshot still shows this menu on, less any a
    /// different menu has since been given -- those are not this menu's to release,
    /// so they neither hold it on the shelf nor block a restore.
    /// </summary>
    private List<Guid> PublishedScreensThisMenuCanStillRelease(Guid menuId) =>
        PublishEvents
            .Where(e => e.MenuId == menuId)
            .OrderByDescending(e => e.Version)
            .Take(1)
            .SelectMany(e => MenuSnapshot.Parse(e.Snapshot)?.Screens ?? [])
            .Select(screen => screen.ScreenId)
            .Where(screenId => !Assignments.Any(a => a.ScreenId == screenId && a.MenuId != menuId))
            .Distinct()
            .ToList();

    public Task<PutAwayOutcome> SetPutAwayAsync(
        Guid venueId,
        Guid menuId,
        bool isPutAway,
        int activeMenuLimit,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default)
    {
        var active = MenuCount - PutAwayMenus.Count;
        if (PutAwayMenus.Contains(menuId) == isPutAway)
        {
            return Task.FromResult(new PutAwayOutcome(PutAwayOutcomes.Unchanged, active));
        }

        if (!isPutAway && active + 1 > activeMenuLimit)
        {
            return Task.FromResult(new PutAwayOutcome(PutAwayOutcomes.OverCeiling, active));
        }

        // Being on a screen is not the presence of an assignment row: a take-off
        // reaches the screens only on the next publish, so the published snapshot
        // is asked too. Otherwise a menu is put away with its take-off pending and
        // the publish that would free the screen is refused for being put away.
        if (isPutAway
            && (Assignments.Any(a => a.VenueId == venueId && a.MenuId == menuId)
                || PublishedScreensThisMenuCanStillRelease(menuId).Count > 0))
        {
            return Task.FromResult(new PutAwayOutcome(PutAwayOutcomes.StillOnScreens, active));
        }

        if (isPutAway)
        {
            PutAwayMenus.Add(menuId);
        }
        else
        {
            PutAwayMenus.Remove(menuId);
        }

        History.Add(new MenuHistoryEntry
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            MenuId = menuId,
            Kind = isPutAway ? MenuHistoryKinds.PutAway : MenuHistoryKinds.PutBack,
            Detail = detail,
            Author = author,
            OccurredUtc = occurredUtc
        });

        return Task.FromResult(new PutAwayOutcome(PutAwayOutcomes.Changed, MenuCount - PutAwayMenus.Count));
    }

    public Task<IReadOnlyCollection<MenuScreenAssignment>> GetAssignmentsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuScreenAssignment>>(
            Assignments.Where(a => a.VenueId == venueId).ToArray());

    // ----- Consolidated editor writes -----

    public Menu? CreatedMenu { get; private set; }

    public Task<MenuCreateOutcome> CreateMenuWithinCeilingAsync(
        Menu menu,
        int activeMenuLimit,
        CancellationToken cancellationToken = default)
    {
        if (MenuCount + 1 > activeMenuLimit)
        {
            return Task.FromResult(new MenuCreateOutcome(false, MenuCount));
        }

        MenuCount++;
        CreatedMenu = menu;
        return Task.FromResult(new MenuCreateOutcome(true, MenuCount));
    }

    public Task<ItemPlacementOutcome> CreateItemOnMenuAsync(
        Item item,
        Guid menuId,
        Guid sectionId,
        int itemsPerMenuLimit,
        CancellationToken cancellationToken = default)
    {
        if (!Sections.Any(section => section.Id == sectionId && section.MenuId == menuId && section.VenueId == item.VenueId && section.IsActive))
        {
            return Task.FromResult(new ItemPlacementOutcome(ItemPlacementOutcomes.SectionMissing, 0, 0));
        }

        var onMenu = Placements
            .Where(placement => placement.VenueId == item.VenueId && placement.MenuId == menuId)
            .Select(placement => placement.ItemId)
            .Distinct()
            .Count();
        if (onMenu + 1 > itemsPerMenuLimit)
        {
            return Task.FromResult(new ItemPlacementOutcome(ItemPlacementOutcomes.OverCeiling, onMenu, 0));
        }

        item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
        Items.Add(item);
        var sortOrder = Placements
            .Where(placement => placement.MenuSectionId == sectionId)
            .Select(placement => placement.SortOrder + 1)
            .DefaultIfEmpty(0)
            .Max();
        Placements.Add(new Placement
        {
            Id = Guid.NewGuid(),
            VenueId = item.VenueId,
            MenuId = menuId,
            MenuSectionId = sectionId,
            ItemId = item.Id,
            SortOrder = sortOrder
        });
        return Task.FromResult(new ItemPlacementOutcome(ItemPlacementOutcomes.Created, onMenu + 1, sortOrder));
    }

    public Task<int> ReorderPlacementsAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime updatedUtc,
        CancellationToken cancellationToken = default)
    {
        var ordered = itemIds.ToList();
        var changed = 0;
        foreach (var placement in Placements.Where(p => p.VenueId == venueId && p.MenuId == menuId && p.MenuSectionId == sectionId))
        {
            var index = ordered.IndexOf(placement.ItemId);
            if (index >= 0)
            {
                placement.SortOrder = index;
                placement.UpdatedUtc = updatedUtc;
                changed++;
            }
        }

        return Task.FromResult(changed);
    }

    public Task<IReadOnlyCollection<PlacedMenuItem>> GetPlacedItemsForVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<PlacedMenuItem>>(Placements
            .Where(placement => placement.VenueId == venueId)
            .OrderBy(placement => placement.MenuSectionId)
            .ThenBy(placement => placement.SortOrder)
            .Select(placement =>
            {
                var item = Items.Single(candidate => candidate.Id == placement.ItemId);
                return new PlacedMenuItem
                {
                    MenuId = placement.MenuId,
                    MenuSectionId = placement.MenuSectionId,
                    ItemId = placement.ItemId,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    SortOrder = placement.SortOrder,
                    IsAvailable = Availability
                        .Where(state => state.VenueId == venueId && state.ItemId == placement.ItemId)
                        .Select(state => state.IsAvailable)
                        .DefaultIfEmpty(true)
                        .Single(),
                    IsActive = item.IsActive,
                    CreatedUtc = placement.CreatedUtc,
                    UpdatedUtc = placement.UpdatedUtc
                };
            })
            .ToArray());

    // ----- Publish and history -----

    /// <summary>The snapshot this fake returns for the working state.</summary>
    public string? WorkingSnapshotJson { get; set; }

    /// <summary>
    /// The working snapshot as the statement rebuilds it, with its screens read
    /// from the live assignments rather than from whatever the stored shape was
    /// last set to. Holding the two apart is the point of the model: a take-off
    /// leaves the working state at once, and reaches the screens only when a
    /// publish carries it (Q68). A fake that kept the screen in both could not
    /// tell those two states apart.
    /// </summary>
    private string? WorkingSnapshotNow(Guid menuId)
    {
        var parsed = MenuSnapshot.Parse(WorkingSnapshotJson);
        if (parsed is null)
        {
            return WorkingSnapshotJson;
        }

        parsed.Screens = Assignments
            .Where(a => a.MenuId == menuId)
            .Select(a => new SnapshotScreen { ScreenId = a.ScreenId })
            .ToList();
        return MenuSnapshot.Serialize(parsed);
    }

    public Task<string?> GetWorkingSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(WorkingSnapshotNow(menuId));

    public Task<string?> GetLatestPublishedSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(PublishEvents
            .Where(e => e.VenueId == venueId && e.MenuId == menuId)
            .OrderByDescending(e => e.Version)
            .Select(e => e.Snapshot)
            .FirstOrDefault());

    public async Task<DraftSnapshots> GetDraftSnapshotsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        new(
            await GetLatestPublishedSnapshotAsync(venueId, menuId, cancellationToken).ConfigureAwait(false),
            WorkingSnapshotNow(menuId),
            PublishEvents.Where(e => e.VenueId == venueId && e.MenuId == menuId).Select(e => e.Version).DefaultIfEmpty(0).Max());

    /// <summary>
    /// Set to make the next publish observe a working state different from the one
    /// the caller's diff came from, the way a concurrent edit would.
    /// </summary>
    public string? WorkingSnapshotAtPublish { get; set; }

    public Task RestoreSnapshotAsync(
        Guid venueId,
        Guid menuId,
        string snapshotJson,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default,
        string kind = MenuHistoryKinds.Restored)
    {
        // A restore puts back whatever shape it was given, screens included, so a
        // version that was on a screen is a third door onto the shelf. A shape with
        // no screens in it cannot put a menu back on one, so discarding a draft on
        // a shelved menu stays possible.
        if (PutAwayMenus.Contains(menuId) && MenuSnapshot.Parse(snapshotJson)?.Screens?.Count > 0)
        {
            throw new MenuPutAwayException(
                "This menu is put away. Put it back on the shelf before going back to a version it was on a screen for.");
        }

        // The statement puts the working rows back, screen assignments included --
        // which is exactly why the refusal above exists.
        WorkingSnapshotJson = snapshotJson;
        Assignments.RemoveAll(a => a.VenueId == venueId && a.MenuId == menuId);
        foreach (var screen in MenuSnapshot.Parse(snapshotJson)?.Screens ?? [])
        {
            Assignments.RemoveAll(a => a.ScreenId == screen.ScreenId);
            Assignments.Add(new MenuScreenAssignment
            {
                Id = Guid.NewGuid(),
                VenueId = venueId,
                ScreenId = screen.ScreenId,
                MenuId = menuId
            });
        }

        History.Add(new MenuHistoryEntry
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            MenuId = menuId,
            Kind = kind,
            Detail = detail,
            Author = author,
            OccurredUtc = occurredUtc
        });
        return Task.CompletedTask;
    }

    public Task<PublishOutcome> PublishAsync(
        MenuPublishEvent publishEvent,
        int changeCount,
        string? shippedChanges,
        string expectedWorkingSnapshot,
        string? expectedPublishedSnapshot,
        long expectedPublishedVersion,
        CancellationToken cancellationToken = default)
    {
        // A put-away menu is off the shelf; publishing is not a way back on.
        if (PutAwayMenus.Contains(publishEvent.MenuId))
        {
            throw new MenuPutAwayException("This menu is put away. Put it back on the shelf before publishing it.");
        }

        // The statement rebuilds the working snapshot under lock and refuses if it
        // has moved, and refuses again if someone else published in between. The
        // fake mirrors both, so the retry path is exercised here. The comparison is
        // ordinal because the statement's is binary.
        var observed = WorkingSnapshotAtPublish ?? WorkingSnapshotNow(publishEvent.MenuId);
        var currentVersion = PublishEvents
            .Where(e => e.VenueId == publishEvent.VenueId && e.MenuId == publishEvent.MenuId)
            .Select(e => e.Version)
            .DefaultIfEmpty(0)
            .Max();
        var currentPublished = PublishEvents
            .Where(e => e.VenueId == publishEvent.VenueId && e.MenuId == publishEvent.MenuId)
            .OrderByDescending(e => e.Version)
            .Select(e => e.Snapshot)
            .FirstOrDefault();
        if (!string.Equals(observed, expectedWorkingSnapshot, StringComparison.Ordinal)
            || currentVersion != expectedPublishedVersion
            || !string.Equals(currentPublished, expectedPublishedSnapshot, StringComparison.Ordinal))
        {
            WorkingSnapshotJson = observed;
            WorkingSnapshotAtPublish = null;
            throw new MenuMovedWhilePublishingException("The menu changed while it was being published.");
        }

        var assigned = Assignments
            .Where(a => a.VenueId == publishEvent.VenueId && a.MenuId == publishEvent.MenuId)
            .Select(a => a.ScreenId)
            .ToList();

        // Membership comes from the previous published snapshot, never from the
        // delivery rows: a target records who was told, including screens a
        // take-off released.
        var previouslyTargeted = PublishEvents
            .Where(e => e.MenuId == publishEvent.MenuId)
            .OrderByDescending(e => e.Version)
            .Take(1)
            .SelectMany(e => MenuSnapshot.Parse(e.Snapshot)?.Screens ?? [])
            .Select(screen => screen.ScreenId)
            .ToList();

        // A screen another menu has since been given is not this publish's to
        // touch, and is reported rather than silently dropped.
        var conflicted = previouslyTargeted
            .Where(screenId => Assignments.Any(a => a.ScreenId == screenId && a.MenuId != publishEvent.MenuId))
            .Distinct()
            .ToArray();
        var targets = assigned.Union(previouslyTargeted.Except(conflicted)).ToList();

        if (targets.Count == 0)
        {
            // Q80 lives in the statement, so the fake mirrors it — as does the
            // distinct "every screen was taken" refusal.
            throw conflicted.Length > 0
                ? new ScreensTakenByAnotherMenuException(
                    "Every screen this menu was on is now showing a different menu, so this publish would reach nothing.")
                : new MenuNotOnAnyScreenException(
                    "Pair a screen to publish. This menu is not on a screen yet, so publishing it would reach nothing.");
        }

        publishEvent.Id = publishEvent.Id == Guid.Empty ? Guid.NewGuid() : publishEvent.Id;
        publishEvent.Version = PublishEvents
            .Where(e => e.MenuId == publishEvent.MenuId)
            .Select(e => e.Version)
            .DefaultIfEmpty(0)
            .Max() + 1;
        publishEvent.ChangeCount = changeCount;
        publishEvent.ShippedChanges = shippedChanges;
        publishEvent.Snapshot = observed;

        PublishEvents.Add(publishEvent);

        foreach (var screenId in targets)
        {
            PublishTargets.Add(new MenuPublishTarget
            {
                Id = Guid.NewGuid(),
                PublishEventId = publishEvent.Id,
                ScreenId = screenId,
                State = MenuPublishTargetStates.Pending,
                UpdatedUtc = publishEvent.PublishedUtc
            });
        }

        History.Add(new MenuHistoryEntry
        {
            Id = Guid.NewGuid(),
            VenueId = publishEvent.VenueId,
            MenuId = publishEvent.MenuId,
            Kind = MenuHistoryKinds.Published,
            PublishEventId = publishEvent.Id,
            Author = publishEvent.Author,
            OccurredUtc = publishEvent.PublishedUtc
        });

        // The publish that carries a take-off records it under its own name.
        if (assigned.Count == 0)
        {
            History.Add(new MenuHistoryEntry
            {
                Id = Guid.NewGuid(),
                VenueId = publishEvent.VenueId,
                MenuId = publishEvent.MenuId,
                Kind = MenuHistoryKinds.TakenOffScreens,
                PublishEventId = publishEvent.Id,
                Detail = $"Taken off {targets.Count} screen(s).",
                Author = publishEvent.Author,
                OccurredUtc = publishEvent.PublishedUtc
            });
        }

        return Task.FromResult(new PublishOutcome(publishEvent, conflicted));
    }

    /// <summary>Snapshot the fake hands back from a publish, so restore can be exercised.</summary>
    public string? SnapshotJson { get; set; }

    public Task<IReadOnlyCollection<MenuPublishEvent>> GetPublishHistoryAsync(
        Guid venueId,
        Guid menuId,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuPublishEvent>>(
            PublishEvents
                .Where(e => e.VenueId == venueId && e.MenuId == menuId)
                .OrderByDescending(e => e.Version)
                .Take(limit)
                .ToArray());

    public Task<MenuPublishEvent?> GetPublishEventAsync(Guid venueId, Guid menuId, long version, CancellationToken cancellationToken = default) =>
        Task.FromResult(PublishEvents.SingleOrDefault(e => e.VenueId == venueId && e.MenuId == menuId && e.Version == version));

    public Task<IReadOnlyCollection<MenuPublishTarget>> GetPublishTargetsAsync(Guid publishEventId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuPublishTarget>>(
            PublishTargets.Where(target => target.PublishEventId == publishEventId).ToArray());

    public Task<Guid> RecordHistoryAsync(MenuHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        entry.Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id;
        History.Add(entry);
        return Task.FromResult(entry.Id);
    }

    public Task<IReadOnlyCollection<MenuHistoryEntry>> GetHistoryAsync(
        Guid venueId,
        Guid menuId,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuHistoryEntry>>(
            History
                .Where(entry => entry.VenueId == venueId && entry.MenuId == menuId)
                .OrderByDescending(entry => entry.OccurredUtc)
                .Take(limit)
                .ToArray());

    // ----- Ceilings -----

    public Task<IReadOnlyDictionary<string, int>> GetCeilingsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyDictionary<string, int>>(Ceilings);

    public Task<int> CountMenusAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult(MenuCount);
}

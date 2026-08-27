using Vennu.Core.Models;
using Vennu.Api.Services;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for the menu content repository. It deliberately mirrors the
/// invariants the SQL enforces: the Q80 refusal and the ceiling checks live
/// inside the same operations that write, and every read is venue-scoped.
/// </summary>
internal sealed class FakeContentRepository : IContentRepository
{
    public List<MenuPage> Pages { get; } = [];

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
    public int? TransitionItemsPerMenuLimit { get; private set; }
    public ReorderOutcome TransitionOutcome { get; set; } = new(ReorderOutcomes.Reordered, 1);

    public List<Guid> ResetAutomationVenues { get; } = [];

    // ----- Library and placements -----

    public Task<IReadOnlyCollection<MenuPage>> GetPagesAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuPage>>(Pages.Where(page => page.VenueId == venueId && page.MenuId == menuId).OrderBy(page => page.SortOrder).ToArray());

    public Task<MenuPage?> CreatePageAsync(Guid venueId, Guid menuId, Guid pageId, string name, DateTime now, CancellationToken cancellationToken = default)
    {
        var page = new MenuPage { Id = pageId, VenueId = venueId, MenuId = menuId, Name = name, SortOrder = Pages.Count(candidate => candidate.VenueId == venueId && candidate.MenuId == menuId), CreatedUtc = now, UpdatedUtc = now };
        Pages.Add(page);
        return Task.FromResult<MenuPage?>(page);
    }

    public Task<bool> RenamePageAsync(Guid venueId, Guid menuId, Guid pageId, string name, DateTime now, CancellationToken cancellationToken = default)
    {
        var page = Pages.SingleOrDefault(candidate => candidate.Id == pageId && candidate.VenueId == venueId && candidate.MenuId == menuId);
        if (page is null) return Task.FromResult(false);
        page.Name = name; page.UpdatedUtc = now; return Task.FromResult(true);
    }

    public Task<ReorderOutcome> ReorderPagesGuardedAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> pageIds, DateTime now, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ReorderOutcome("reordered", 0));

    public Task<MenuPage?> DuplicatePageAsync(Guid venueId, Guid menuId, Guid sourcePageId, Guid newPageId, DateTime now, CancellationToken cancellationToken = default)
    {
        var source = Pages.SingleOrDefault(page => page.Id == sourcePageId && page.VenueId == venueId && page.MenuId == menuId);
        if (source is null) return Task.FromResult<MenuPage?>(null);
        var copy = new MenuPage { Id = newPageId, VenueId = venueId, MenuId = menuId, Name = source.Name + " copy", SortOrder = source.SortOrder + 1, CreatedUtc = now, UpdatedUtc = now };
        Pages.Add(copy); return Task.FromResult<MenuPage?>(copy);
    }

    public Task<PageDeleteOutcome> DeletePageAsync(Guid venueId, Guid menuId, Guid pageId, Guid? moveSectionsToPageId, bool deleteSections = false, CancellationToken cancellationToken = default)
    {
        var removed = Pages.RemoveAll(page => page.Id == pageId && page.VenueId == venueId && page.MenuId == menuId);
        return Task.FromResult(new PageDeleteOutcome(removed == 0 ? "page_missing" : "deleted", 0, 0));
    }

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

    /// <summary>
    /// Mirrors the guarded SQL, including its treatment of NULL and empty as the
    /// same absence — a comparison that differs between here and the database would
    /// make these tests agree with something the product does not do.
    /// </summary>
    public Task<ItemUpdateOutcome> UpdateItemValuesGuardedAsync(
        Guid venueId,
        Guid itemId,
        string name,
        string? description,
        string? price,
        ItemValueExpectation? expected,
        DateTime now,
        CancellationToken cancellationToken = default,
        Guid? menuId = null,
        bool isListed = true,
        Guid? sectionId = null)
    {
        var item = Items.SingleOrDefault(candidate => candidate.VenueId == venueId && candidate.Id == itemId);
        if (item is null)
        {
            return Task.FromResult(new ItemUpdateOutcome("not_found", null, null, null, null));
        }

        // The placement this edit is about, resolved the same way the SQL resolves
        // it (A19). More than one answering to the same menu is the two-sections
        // case, and it is refused rather than guessed.
        var matching = menuId is null
            ? []
            : Placements
                .Where(candidate => candidate.VenueId == venueId
                    && candidate.ItemId == itemId
                    && candidate.MenuId == menuId
                    && (sectionId is null || candidate.MenuSectionId == sectionId))
                .OrderBy(candidate => candidate.MenuSectionId)
                .ToArray();
        if (matching.Length > 1)
        {
            return Task.FromResult(new ItemUpdateOutcome(
                "placement_ambiguous", item.Name, item.Description, item.Price, item.IsListed));
        }

        var placement = matching.FirstOrDefault();
        var effectivePrice = placement?.ImportedPriceOverride ?? item.Price;

        static bool Same(string? left, string? right) => (left ?? string.Empty) == (right ?? string.Empty);

        if (expected is not null
            && (item.Name != expected.Name
                || !Same(item.Description, expected.Description)
                || !Same(effectivePrice, expected.Price)
                || item.IsListed != expected.IsListed))
        {
            return Task.FromResult(new ItemUpdateOutcome("item_changed", item.Name, item.Description, effectivePrice, item.IsListed));
        }

        item.Name = name;
        item.Description = description;
        item.IsListed = isListed;
        item.UpdatedUtc = now;

        // Price follows the placement when the edit was made from one (A19); the
        // library default is only written when the edit belongs to no menu.
        if (placement is null)
        {
            item.Price = price;
        }
        else
        {
            placement.ImportedPriceOverride = price;
            placement.UpdatedUtc = now;
        }

        return Task.FromResult(new ItemUpdateOutcome("updated", name, description, price, isListed));
    }

    public Task<IReadOnlyCollection<Item>> GetItemsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Item>>(Items.Where(item => item.VenueId == venueId).ToArray());

    public Task<int> CountActiveMenusAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Menus.Count(menu => menu.VenueId == venueId && !menu.IsPutAway));

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

    public Task<IReadOnlyCollection<ItemAvailability>> RestoreAllAvailabilityAsync(Guid venueId, DateTime changedUtc, string? changedBy, CancellationToken cancellationToken = default)
    {
        var published = PublishEvents.Where(entry => entry.VenueId == venueId)
            .SelectMany(entry => MenuSnapshot.Parse(entry.Snapshot)?.Sections ?? [])
            .SelectMany(section => section.Items ?? []).Select(item => item.ItemId).ToHashSet();
        var restored = Availability.Where(state => state.VenueId == venueId && published.Contains(state.ItemId) && !state.IsAvailable).ToArray();
        foreach (var state in restored)
        {
            state.IsAvailable = true;
            state.ChangedUtc = changedUtc;
            state.ChangedBy = changedBy;
        }
        return Task.FromResult<IReadOnlyCollection<ItemAvailability>>(restored);
    }

    // ----- Assignment -----

    public Task<MenuScreenAssignment> AssignScreenAsync(MenuScreenAssignment assignment, CancellationToken cancellationToken = default)
    {
        assignment.Id = assignment.Id == Guid.Empty ? Guid.NewGuid() : assignment.Id;
        Assignments.RemoveAll(existing => existing.ScreenId == assignment.ScreenId && existing.PageId == assignment.PageId);
        Assignments.Add(assignment);
        return Task.FromResult(assignment);
    }

    public Task<bool> ClearScreenAssignmentAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Assignments.RemoveAll(a => a.VenueId == venueId && a.ScreenId == screenId) > 0);

    public Task<bool> ClearPageScreenAssignmentAsync(Guid venueId, Guid screenId, Guid menuId, Guid pageId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Assignments.RemoveAll(a => a.VenueId == venueId && a.ScreenId == screenId && a.MenuId == menuId && a.PageId == pageId) > 0);

    public Task ApplyPageScreenAssignmentsAsync(Guid venueId, Guid menuId, IReadOnlyCollection<PageScreenAssignmentChange> changes, string? author, DateTime occurredUtc, CancellationToken cancellationToken = default)
    {
        if (changes.Any(change => change.ScreenId == Guid.Empty))
            throw new InvalidOperationException("The screen assignment changed before it could be saved. Nothing was changed.");
        foreach (var change in changes)
        {
            if (change.Mode == "remove") Assignments.RemoveAll(a => a.VenueId == venueId && a.ScreenId == change.ScreenId && a.MenuId == menuId && a.PageId == change.PageId);
            else
            {
                if (change.Mode == "replace") Assignments.RemoveAll(a => a.VenueId == venueId && a.ScreenId == change.ScreenId && a.PageId != change.PageId);
                Assignments.RemoveAll(a => a.VenueId == venueId && a.ScreenId == change.ScreenId && a.PageId == change.PageId);
                Assignments.Add(new MenuScreenAssignment { Id = Guid.NewGuid(), VenueId = venueId, ScreenId = change.ScreenId, MenuId = menuId, PageId = change.PageId, AssignedBy = author, AssignedUtc = occurredUtc });
            }
        }
        return Task.CompletedTask;
    }

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
        CancellationToken cancellationToken = default,
        string? author = null)
    {
        if (!Sections.Any(section => section.Id == sectionId && section.MenuId == menuId && section.VenueId == item.VenueId))
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

    public Task<IReadOnlyCollection<ScreenShowing>> GetScreensShowingAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        // Derived the way the statement derives it: the publish that last spoke to the
        // screen, and whether its snapshot still names the screen.
        var showing = PublishTargets
            .Select(target => new { target.ScreenId, Event = PublishEvents.SingleOrDefault(e => e.Id == target.PublishEventId) })
            .Where(pair => pair.Event is not null && pair.Event!.VenueId == venueId)
            .GroupBy(pair => pair.ScreenId)
            .Select(group => group.OrderByDescending(pair => pair.Event!.PublishedUtc).First())
            .Select(pair => new ScreenShowing(
                pair.ScreenId,
                pair.ScreenId.ToString(),
                null,
                "Offline",
                null,
                1920,
                1080,
                StillNames(pair.Event!, pair.ScreenId) ? pair.Event!.MenuId : null,
                StillNames(pair.Event!, pair.ScreenId) ? pair.Event!.MenuId.ToString() : null,
                StillNames(pair.Event!, pair.ScreenId) ? pair.Event!.Version : null,
                StillNames(pair.Event!, pair.ScreenId) ? pair.Event!.PublishedUtc : null,
                StillNames(pair.Event!, pair.ScreenId) ? pair.Event!.Author : null))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<ScreenShowing>>(showing);

        static bool StillNames(MenuPublishEvent publishEvent, Guid screenId) =>
            (MenuSnapshot.Parse(publishEvent.Snapshot)?.Screens ?? []).Any(screen => screen.ScreenId == screenId);
    }

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

    public Task<PublishedBoard?> GetLatestPublishedBoardAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(PublishEvents
            .Where(e => e.VenueId == venueId && e.MenuId == menuId)
            .OrderByDescending(e => e.Version)
            .Select(e => new PublishedBoard(e.Snapshot, e.Version, e.PublishedUtc, e.Author))
            .FirstOrDefault());

    /// <summary>The venue's menus. Storage only — nothing here decides anything.</summary>
    public List<Menu> Menus { get; } = [];

    public Task<IReadOnlyCollection<ShelfMenu>> GetShelfAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ShelfMenu>>(Menus
            .Where(menu => menu.VenueId == venueId)
            .OrderBy(menu => menu.Name, StringComparer.Ordinal)
            .Select(menu =>
            {
                var latest = PublishEvents
                    .Where(e => e.VenueId == venueId && e.MenuId == menu.Id)
                    .OrderByDescending(e => e.Version)
                    .FirstOrDefault();

                return new ShelfMenu(
                    menu.Id,
                    menu.Name,
                    menu.Theme,
                    menu.IsPutAway,
                    latest?.Version,
                    latest?.PublishedUtc,
                    latest?.Author,
                    latest?.Snapshot,
                    WorkingSnapshotNow(menu.Id));
            })
            .ToArray());

    /// <summary>
    /// Present so this class satisfies the interface, and deliberately not
    /// implemented. Everything duplicate does that is worth asserting — the ceiling
    /// under the lock, the name chosen against what already exists, the placements
    /// pointing at the same library items — is enforced in SQL. An in-memory version
    /// would be a second implementation of those rules, and a test against it would
    /// prove the copy rather than the product. That is exactly how a defect survived
    /// 412 green unit tests in milestone 1.
    /// </summary>
    // ----- The builder's writes -----
    //
    // Every rule the builder leans on is decided inside the statement that writes:
    // the next sort order under a lock, the ceiling under a lock, "already on this
    // board", and whether the caller's reorder list still matches the menu. A
    // double that re-implemented any of them would prove the copy, and the copy
    // drifts — which is how a defect survived 412 green unit tests once already.
    // These refuse and name where the rule actually lives.

    public Task<SectionCreateOutcome> CreateSectionOnMenuAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        DateTime now,
        Guid? pageId = null,
        string? author = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "Adding a section reads its sort order under the lock that inserts it. "
            + "Assert it in Vennu.Data.IntegrationTests against a real database.");

    public Task<bool> RenameSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        DateTime now,
        string? author = null,
        CancellationToken cancellationToken = default)
    {
        var section = Sections.SingleOrDefault(item =>
            item.Id == sectionId && item.MenuId == menuId && item.VenueId == venueId);
        if (section is null)
        {
            return Task.FromResult(false);
        }

        section.Name = name;
        section.UpdatedUtc = now;
        return Task.FromResult(true);
    }

    public Task<SectionDeleteOutcome> DeleteSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid? moveItemsToSectionId,
        bool deletePlacements,
        string? author = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "Deleting a section releases its placements in the same transaction. "
            + "Assert it in Vennu.Data.IntegrationTests against a real database.");

    public Task<SectionPageMoveOutcome> MoveSectionToPageAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid destinationPageId,
        string? author = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "Moving a section relies on FK_Placements_SectionOnPage's ON UPDATE CASCADE. "
            + "Assert it in Vennu.Data.IntegrationTests against a real database.");

    public Task<ReorderOutcome> ReorderSectionsGuardedAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        DateTime now,
        string? author = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "Reorder proves the caller's list still matches the menu under the lock that writes it. "
            + "Assert it in Vennu.Data.IntegrationTests against a real database.");

    public Task<ReorderOutcome> ReorderPlacementsGuardedAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime now,
        CancellationToken cancellationToken = default,
        string? author = null) =>
        throw new NotSupportedException(
            "Reorder proves the caller's list still matches the section under the lock that writes it. "
            + "Assert it in Vennu.Data.IntegrationTests against a real database.");

    public Task<ReorderOutcome> MovePlacementGuardedAsync(
        Guid venueId,
        Guid menuId,
        Guid itemId,
        Guid sourceSectionId,
        Guid destinationSectionId,
        IReadOnlyCollection<Guid> sourceItemIds,
        IReadOnlyCollection<Guid> destinationItemIds,
        DateTime now,
        CancellationToken cancellationToken = default,
        string? author = null) =>
        throw new NotSupportedException(
            "Cross-section movement proves both orders and commits the placement and history under one lock. "
            + "Assert it in Vennu.Data.IntegrationTests against a real database.");

    public Task<PlaceExistingOutcome> PlaceExistingItemAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        int itemsPerMenuLimit,
        DateTime now,
        CancellationToken cancellationToken = default,
        string? author = null) =>
        throw new NotSupportedException(
            "Placing an existing item decides 'already on this board' and the ceiling under one lock. "
            + "Assert it in Vennu.Data.IntegrationTests against a real database.");

    public Task<ReorderOutcome> TransitionPlacementGuardedAsync(
        Guid venueId, Guid menuId, Guid pageId, Guid sectionId, Guid itemId,
        IReadOnlyCollection<Guid> expectedItemIds, IReadOnlyCollection<Guid> desiredItemIds,
        int itemsPerMenuLimit, DateTime now, CancellationToken cancellationToken = default, string? author = null)
    {
        TransitionItemsPerMenuLimit = itemsPerMenuLimit;
        return Task.FromResult(TransitionOutcome);
    }

    public Task<bool> RemoveItemFromPageAsync(
        Guid venueId,
        Guid menuId,
        Guid pageId,
        Guid itemId,
        DateTime now,
        CancellationToken cancellationToken = default,
        string? author = null)
    {
        var removed = Placements.RemoveAll(placement =>
            placement.VenueId == venueId && placement.MenuId == menuId
            && placement.PageId == pageId && placement.ItemId == itemId);
        return Task.FromResult(removed > 0);
    }

    public Task<IReadOnlyCollection<Item>> SearchItemsAsync(
        Guid venueId,
        string? query,
        int take,
        CancellationToken cancellationToken = default)
    {
        var trimmed = (query ?? string.Empty).Trim();
        IReadOnlyCollection<Item> results =
        [
            .. Items
                .Where(item => item.VenueId == venueId)
                .Where(item => trimmed.Length == 0
                    || item.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Name.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(item => item.Name, StringComparer.Ordinal)
                .Take(take)
        ];
        return Task.FromResult(results);
    }

    public Task<IReadOnlyCollection<ItemBoard>> GetItemBoardsAsync(
        Guid venueId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken = default)
    {
        var wanted = itemIds.ToHashSet();
        IReadOnlyCollection<ItemBoard> boards =
        [
            .. Placements
                .Where(placement => placement.VenueId == venueId && wanted.Contains(placement.ItemId))
                .Select(placement => new ItemBoard(placement.ItemId, placement.MenuId, MenuNames.TryGetValue(placement.MenuId, out var name) ? name : placement.MenuId.ToString()))
        ];
        return Task.FromResult(boards);
    }

    /// <summary>Menu names for <see cref="GetItemBoardsAsync"/>; the SQL joins for them.</summary>
    public Dictionary<Guid, string> MenuNames { get; } = [];

    public Task<MenuDuplicateOutcome> DuplicateMenuWithinCeilingAsync(
        Guid venueId,
        Guid sourceMenuId,
        Guid newMenuId,
        int activeMenuLimit,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "Duplicate is enforced in SQL. Assert it in Vennu.Data.IntegrationTests against a real database.");

    public async Task<DraftSnapshots> GetDraftSnapshotsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default)
    {
        var latest = PublishEvents
            .Where(e => e.VenueId == venueId && e.MenuId == menuId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefault();

        return new(
            await GetLatestPublishedSnapshotAsync(venueId, menuId, cancellationToken).ConfigureAwait(false),
            WorkingSnapshotNow(menuId),
            latest?.Version ?? 0,
            latest?.PublishedUtc,
            latest?.Author);
    }

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
        WorkingSnapshotJson = snapshotJson;

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

    /// <summary>
    /// Makes the next publish fail the way a concurrent change makes the statement
    /// fail. This is a seam, not a rule: the fake is told when to refuse, it does not
    /// decide. Deciding is the database's job, and a second opinion in C# is how a
    /// unit test ends up certifying behaviour the product does not have.
    /// </summary>
    public Exception? FailNextPublishWith { get; set; }

    /// <summary>Runs on every publish. Lets a test fail repeatedly, to exercise the bound on retries.</summary>
    public Action? OnPublish { get; set; }

    public Task<PublishOutcome> PublishAsync(
        MenuPublishEvent publishEvent,
        string? shippedChanges,
        string expectedWorkingSnapshot,
        string? expectedPublishedSnapshot,
        long expectedPublishedVersion,
        CancellationToken cancellationToken = default)
    {
        OnPublish?.Invoke();

        if (FailNextPublishWith is { } failure)
        {
            FailNextPublishWith = null;

            // The caller reads the menu again before retrying, so the working state it
            // will find is whatever moved underneath it.
            if (WorkingSnapshotAtPublish is not null)
            {
                WorkingSnapshotJson = WorkingSnapshotAtPublish;
                WorkingSnapshotAtPublish = null;
            }

            throw failure;
        }

        publishEvent.Id = publishEvent.Id == Guid.Empty ? Guid.NewGuid() : publishEvent.Id;
        publishEvent.Version = PublishEvents
            .Where(e => e.MenuId == publishEvent.MenuId)
            .Select(e => e.Version)
            .DefaultIfEmpty(0)
            .Max() + 1;
        // Derived, exactly as the statement derives it.
        publishEvent.ChangeCount = string.IsNullOrWhiteSpace(shippedChanges)
            ? 0
            : System.Text.Json.JsonSerializer.Deserialize<List<SnapshotChange>>(shippedChanges)?.Count ?? 0;
        publishEvent.ShippedChanges = shippedChanges;
        publishEvent.Snapshot = expectedWorkingSnapshot;

        PublishEvents.Add(publishEvent);

        foreach (var screenId in Assignments
            .Where(a => a.VenueId == publishEvent.VenueId && a.MenuId == publishEvent.MenuId)
            .Select(a => a.ScreenId))
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

        return Task.FromResult(new PublishOutcome(publishEvent, []));
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

    public Task<IReadOnlyCollection<MenuHistoryEntry>> GetPageHistoryAsync(
        Guid venueId,
        Guid menuId,
        Guid pageId,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuHistoryEntry>>(
            History
                .Where(entry => entry.VenueId == venueId && entry.MenuId == menuId && entry.PageId == pageId
                    && entry.Kind != MenuHistoryKinds.Published)
                .OrderByDescending(entry => entry.OccurredUtc)
                .ThenByDescending(entry => entry.Id)
                .Take(limit)
                .ToArray());

    // ----- Ceilings -----

    public Task<IReadOnlyDictionary<string, int>> GetCeilingsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyDictionary<string, int>>(Ceilings);

    public Task<int> CountMenusAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult(MenuCount);

    public Task ResetAutomationVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        ResetAutomationVenues.Add(venueId);
        return Task.CompletedTask;
    }
}

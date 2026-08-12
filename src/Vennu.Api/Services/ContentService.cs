using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Services;

/// <summary>
/// The Menus save model. Two rules govern everything here:
/// availability is a fact that commits instantly, and everything else is an
/// intention that waits in the menu's draft until someone publishes it.
/// </summary>
public sealed class ContentService(
    IContentRepository library,
    IVenueRepository venues,
    Vennu.Api.Notifications.IScreenUpdateNotifier notifier,
    TimeProvider timeProvider)
{
    /// <summary>
    /// Turns an item on or off for a venue. Never queues, never waits for a
    /// publish, and survives one. Returns the screens the change reaches now.
    /// </summary>
    public async Task<AvailabilityResult> SetAvailabilityAsync(
        Guid venueId,
        Guid itemId,
        bool isAvailable,
        string? changedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await library.GetItemAsync(venueId, itemId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Item '{itemId}' does not belong to venue '{venueId}'.");

        var availability = await library.SetAvailabilityAsync(
            new ItemAvailability
            {
                VenueId = venueId,
                ItemId = itemId,
                IsAvailable = isAvailable,
                ChangedUtc = timeProvider.GetUtcNow().UtcDateTime,
                ChangedBy = changedBy
            },
            cancellationToken).ConfigureAwait(false);

        // The honest count is every screen showing this item through any menu.
        var placements = await library.GetPlacementsForItemAsync(venueId, itemId, cancellationToken).ConfigureAwait(false);
        var assignments = await library.GetAssignmentsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var menuIdsShowingItem = placements.Select(placement => placement.MenuId).ToHashSet();

        var screenIds = assignments
            .Where(assignment => menuIdsShowingItem.Contains(assignment.MenuId))
            .Select(assignment => assignment.ScreenId)
            .ToArray();

        // Telling the caller which screens are affected is not the same as changing
        // them. Push the change out, per screen and once for the venue, so the
        // reported reach is something that actually happened rather than a claim.
        foreach (var screenId in screenIds)
        {
            await notifier
                .NotifyScreenItemAvailabilityChangedAsync(screenId, itemId.ToString(), isAvailable, cancellationToken)
                .ConfigureAwait(false);
        }

        await notifier
            .NotifyVenueItemAvailabilityChangedAsync(venueId, itemId.ToString(), isAvailable, cancellationToken)
            .ConfigureAwait(false);

        return new AvailabilityResult(item, availability, screenIds);
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions =
        new(System.Text.Json.JsonSerializerDefaults.Web);

    /// <summary>
    /// What is different between this menu and the one its screens are showing.
    /// The draft is computed, never authored: no caller supplies a previous value,
    /// so the count cannot disagree with what a publish will ship, and a stale
    /// client cannot misreport or erase someone else's edit (Q182).
    /// </summary>
    public async Task<IReadOnlyList<SnapshotChange>> GetDraftAsync(
        Guid venueId,
        Guid menuId,
        CancellationToken cancellationToken = default) =>
        (await ReadDraftAsync(venueId, menuId, cancellationToken).ConfigureAwait(false)).Changes;

    /// <summary>
    /// The draft, together with the working snapshot it was computed from. Publish
    /// needs both: the snapshot is what it proves has not moved before recording
    /// the changes as shipped.
    /// </summary>
    private async Task<DraftReading> ReadDraftAsync(
        Guid venueId,
        Guid menuId,
        CancellationToken cancellationToken)
    {
        // Both halves come from one read, so a publish landing between two separate
        // reads cannot produce a diff against a version that is already gone.
        var snapshots = await library.GetDraftSnapshotsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        if (snapshots.Working is null)
        {
            throw new InvalidOperationException($"Menu '{menuId}' does not belong to venue '{venueId}'.");
        }

        return new DraftReading(
            MenuSnapshot.Diff(snapshots.Published, snapshots.Working),
            snapshots.Working,
            snapshots.Published,
            snapshots.PublishedVersion);
    }

    private sealed record DraftReading(
        IReadOnlyList<SnapshotChange> Changes,
        string WorkingSnapshot,
        string? PublishedSnapshot,
        long PublishedVersion);

    /// <summary>
    /// Ships the menu as it stands to its screens. Refused when the menu is on no
    /// screen and has none to release — a publish that reaches nothing is a named
    /// state, not a silent version bump (Q80, enforced inside the transaction).
    /// </summary>
    /// <summary>How many times a publish re-reads and retries when the menu moves underneath it.</summary>
    private const int PublishAttempts = 4;

    public async Task<PublishResult> PublishAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        // The shipped set and the snapshot that ships have to be one observation,
        // or history can describe content that never went out. The diff is computed
        // from a working snapshot, and the publish refuses unless the menu is still
        // exactly that snapshot when it commits; if someone edited in between, the
        // whole thing is recomputed rather than recorded wrongly (Q182).
        for (var attempt = 1; ; attempt++)
        {
            var draft = await ReadDraftAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
            var shippedJson = System.Text.Json.JsonSerializer.Serialize(draft.Changes, JsonOptions);

            PublishOutcome outcome;
            try
            {
                outcome = await library.PublishAsync(
                    new MenuPublishEvent
                    {
                        VenueId = venueId,
                        MenuId = menuId,
                        Author = author,
                        PublishedUtc = timeProvider.GetUtcNow().UtcDateTime
                    },
                    shippedJson,
                    draft.WorkingSnapshot,
                    draft.PublishedSnapshot,
                    draft.PublishedVersion,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (MenuMovedWhilePublishingException) when (attempt < PublishAttempts)
            {
                // Someone edited between the diff and the commit. Read the menu
                // again and ship what it actually is now.
                continue;
            }

            var deliveries = await library.GetPublishTargetsAsync(outcome.Event.Id, cancellationToken).ConfigureAwait(false);
            return new PublishResult(
                outcome.Event,
                outcome.Event.ChangeCount,
                deliveries,
                outcome.ConflictedScreenIds);
        }
    }

    /// <summary>
    /// "Go back to" — phrased as a time, never a version. It puts the menu back to
    /// how it looked at that publish, in one transaction, and leaves it as an
    /// unpublished draft: it is never a second silent path to the screens (Q67).
    /// The count of changes it displaced is returned so the caller can warn before
    /// committing.
    /// </summary>
    public async Task<RestoreResult> GoBackToAsync(
        Guid venueId,
        Guid menuId,
        long version,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var target = await library.GetPublishEventAsync(venueId, menuId, version, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Menu '{menuId}' has no published version {version}.");

        if (string.IsNullOrWhiteSpace(target.Snapshot))
        {
            throw new InvalidOperationException(
                $"Version {version} of menu '{menuId}' has no stored content, so it cannot be restored.");
        }

        var displaced = (await GetDraftAsync(venueId, menuId, cancellationToken).ConfigureAwait(false)).Count;

        // Applying the snapshot and recording the act commit together, so a failure
        // part-way cannot leave the menu half-restored.
        try
        {
            await library.RestoreSnapshotAsync(
                venueId,
                menuId,
                target.Snapshot!,
                author,
                $"Put the menu back to how it looked when version {version} was published, replacing {displaced} unpublished change(s).",
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51005)
        {
            // Refused rather than restored around: a restore that cannot put the
            // screens back has not put the menu back to how it looked.
            throw new ScreensTakenByAnotherMenuException(
                "A screen this menu was on is now showing a different menu, so it cannot go back to how it looked. "
                + "Put that screen back on this menu first, or go back to a version it was not on.");
        }

        var draft = await GetDraftAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        return new RestoreResult(draft, displaced);
    }

    /// <summary>
    /// Puts the menu back to what its screens are showing, throwing away every
    /// unpublished edit. The one irreversible act in the model, so the change and
    /// the record naming who did it commit together (Q207).
    /// </summary>
    public async Task<int> DiscardDraftAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var published = await library.GetLatestPublishedSnapshotAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        if (published is null)
        {
            // Nothing has ever been published, so there is no state to return to.
            return 0;
        }

        var discarded = (await GetDraftAsync(venueId, menuId, cancellationToken).ConfigureAwait(false)).Count;
        if (discarded == 0)
        {
            return 0;
        }

        await library.RestoreSnapshotAsync(
            venueId,
            menuId,
            published,
            author,
            $"Discarded {discarded} unpublished change(s).",
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken,
            MenuHistoryKinds.DraftDiscarded).ConfigureAwait(false);

        return discarded;
    }

    /// <summary>
    /// "Take off the screens" is permanent, so unlike an 86 it does not commit on
    /// confirm. It removes the menu from its screens in the working state, where it
    /// shows up as an unpublished change and reaches the screens on the next
    /// Publish (Q68).
    /// </summary>
    public async Task<int> QueueTakeOffScreensAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default) =>
        await library.TakeOffScreensAsync(
            venueId,
            menuId,
            author,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Puts a menu away, or back on the shelf. Put away is the terminal state for a
    /// menu this build — there is no delete — so it is recorded with its author in
    /// the same transaction, and a menu still on a screen is taken off first rather
    /// than disappearing underneath the person.
    /// </summary>
    public async Task<PutAwayResult> SetPutAwayAsync(
        Guid venueId,
        Guid menuId,
        bool isPutAway,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetResolvedCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var limit = ceilings.TryGetValue(MenuCeilings.MenusPerVenue, out var configured) ? configured : int.MaxValue;

        var outcome = await library.SetPutAwayAsync(
            venueId,
            menuId,
            isPutAway,
            limit,
            author,
            isPutAway ? "Put the menu away." : "Put the menu back on the shelf.",
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);

        return outcome.Outcome switch
        {
            PutAwayOutcomes.NotFound =>
                throw new InvalidOperationException($"Menu '{menuId}' does not belong to venue '{venueId}'."),
            PutAwayOutcomes.OverCeiling =>
                throw new MenuCeilingReachedException(
                    MenuCeilings.DescribeRefusal(MenuCeilings.MenusPerVenue, outcome.ActiveMenuCount + 1, limit)),
            PutAwayOutcomes.StillOnScreens =>
                throw new MenuStillOnScreensException(
                    "This menu is still on a screen. Take it off the screens and publish that, so nothing goes blank without you deciding to."),
            _ => new PutAwayResult(outcome.Outcome == PutAwayOutcomes.Changed, outcome.ActiveMenuCount)
        };
    }

    public async Task<MenuScreenAssignment> AssignAsync(
        Guid venueId,
        Guid screenId,
        Guid menuId,
        Guid pageId,
        bool rotate,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var assignment = await library.AssignScreenAsync(
            new MenuScreenAssignment
            {
                VenueId = venueId,
                ScreenId = screenId,
                MenuId = menuId,
                PageId = pageId,
                Rotate = rotate,
                AssignedUtc = now,
                AssignedBy = author
            },
            cancellationToken).ConfigureAwait(false);

        assignment = (await library.GetAssignmentsAsync(venueId, cancellationToken).ConfigureAwait(false))
            .Single(current => current.ScreenId == screenId && current.MenuId == menuId && current.PageId == pageId);

        await library.RecordHistoryAsync(
            new MenuHistoryEntry
            {
                VenueId = venueId,
                MenuId = menuId,
                Kind = MenuHistoryKinds.Assigned,
                Detail = "Placed on a screen.",
                Author = author,
                OccurredUtc = now
            },
            cancellationToken).ConfigureAwait(false);

        return assignment;
    }

    /// <summary>
    /// Every menu the venue has, as the shelf needs it: the board its screens are
    /// showing, how many changes are waiting, and which screens it is on.
    ///
    /// One repository read for the menus and one for the screens, whatever the menu
    /// count. Asking per menu would be a diff per card on every page load, and the
    /// shelf ships its own scale behaviour this milestone (Q176).
    ///
    /// Screens come from what was published, never from the assignments: a menu can
    /// be assigned to a screen and not yet be on it, which is the entire point of a
    /// deliberate publish.
    /// </summary>
    public async Task<IReadOnlyList<ShelfMenuResult>> GetShelfAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var menus = await library.GetShelfAsync(venueId, cancellationToken).ConfigureAwait(false);
        var showing = await library.GetScreensShowingAsync(venueId, cancellationToken).ConfigureAwait(false);

        var screensByMenu = showing
            .Where(screen => screen.MenuId is not null)
            .GroupBy(screen => screen.MenuId!.Value)
            .ToDictionary(group => group.Key, group => group.Select(screen => screen.ScreenId).ToArray());

        return [.. menus.Select(menu => new ShelfMenuResult(
            menu.MenuId,
            menu.Name,
            menu.Theme,
            menu.IsPutAway,
            menu.PublishedVersion,
            menu.LastPublishedUtc,
            menu.LastPublishedBy,
            // The card's count and the card's board are the same pair of snapshots,
            // so what the card shows and what it says about itself cannot disagree.
            MenuSnapshot.Diff(menu.PublishedSnapshot, menu.WorkingSnapshot),
            screensByMenu.TryGetValue(menu.MenuId, out var screenIds) ? screenIds : [],
            MenuSnapshot.Parse(menu.PublishedSnapshot)))];
    }

    /// <summary>
    /// The board a menu's screens are showing, with the publish that put it there.
    /// Null when the menu has never been published — which is a state the shelf
    /// renders, not an error.
    /// </summary>
    public async Task<PublishedBoardResult?> GetPublishedBoardAsync(
        Guid venueId,
        Guid menuId,
        CancellationToken cancellationToken = default)
    {
        var board = await library.GetLatestPublishedBoardAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        if (board is null)
        {
            return null;
        }

        return new PublishedBoardResult(
            menuId,
            board.Version,
            board.PublishedUtc,
            board.Author,
            MenuSnapshot.Parse(board.Snapshot));
    }

    /// <summary>
    /// Copies a menu onto a new one that has never been published and is on no
    /// screen (Q20). The copy places the same library items, so a later price edit
    /// reaches both boards — sharing is the point of a library.
    /// </summary>
    public async Task<DuplicateResult> DuplicateAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetResolvedCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var limit = ceilings.TryGetValue(MenuCeilings.MenusPerVenue, out var configured) ? configured : int.MaxValue;

        var source = await library.GetShelfAsync(venueId, cancellationToken).ConfigureAwait(false);
        var original = source.FirstOrDefault(menu => menu.MenuId == menuId)
            ?? throw new InvalidOperationException($"Menu '{menuId}' does not belong to venue '{venueId}'.");

        var newMenuId = Guid.NewGuid();
        var outcome = await library.DuplicateMenuWithinCeilingAsync(
            venueId,
            menuId,
            newMenuId,
            limit,
            author,
            $"Duplicated from '{original.Name}'.",
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);

        if (!outcome.Created)
        {
            throw new MenuCeilingReachedException(
                MenuCeilings.DescribeRefusal(MenuCeilings.MenusPerVenue, outcome.ActiveMenuCount + 1, limit));
        }

        // The name is whatever the statement settled on under its lock, not what
        // this method guessed: "Summer Menu copy" may well have become "… copy 3".
        return new DuplicateResult(newMenuId, outcome.Name ?? original.Name, outcome.ActiveMenuCount);
    }

    // ---- the builder --------------------------------------------------------

    /// <summary>
    /// Everything the builder needs to open a menu, from ONE read of the pair of
    /// snapshots: the board it draws is the WORKING state, not the published one
    /// the shelf card draws, and the draft is the difference between them. Reading
    /// them apart is how the canvas and the count start describing different menus.
    /// </summary>
    public async Task<BuilderBoardResult?> GetBuilderBoardAsync(
        Guid venueId,
        Guid menuId,
        CancellationToken cancellationToken = default)
    {
        var snapshots = await library.GetDraftSnapshotsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var working = MenuSnapshot.Parse(snapshots.Working);
        if (working is null)
        {
            return null;
        }

        return new BuilderBoardResult(
            working,
            MenuSnapshot.Diff(snapshots.Published, snapshots.Working),
            snapshots.PublishedVersion == 0 ? null : snapshots.PublishedVersion,
            snapshots.PublishedUtc,
            snapshots.PublishedBy);
    }

    public async Task<SectionCreateOutcome> AddSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        Guid? pageId = null,
        CancellationToken cancellationToken = default)
    {
        var outcome = await library.CreateSectionOnMenuAsync(
            venueId, menuId, sectionId, NormalizeSectionName(name),
            timeProvider.GetUtcNow().UtcDateTime, pageId, cancellationToken).ConfigureAwait(false);

        if (outcome.Outcome == SectionOutcomes.Created)
        {
            await NotifyAsync(venueId, "section-added", menuId, cancellationToken).ConfigureAwait(false);
        }

        return outcome;
    }

    public async Task<bool> RenameSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var renamed = await library.RenameSectionAsync(
            venueId, menuId, sectionId, NormalizeSectionName(name),
            timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);

        if (renamed)
        {
            await NotifyAsync(venueId, "section-renamed", menuId, cancellationToken).ConfigureAwait(false);
        }

        return renamed;
    }

    /// <summary>
    /// Deletes a section, moving its placements to a sibling or releasing them back
    /// to the library in the repository's single transaction.
    /// </summary>
    public async Task<SectionDeleteOutcome> DeleteSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid? moveItemsToSectionId,
        bool deletePlacements,
        CancellationToken cancellationToken = default)
    {
        var outcome = await library
            .DeleteSectionAsync(venueId, menuId, sectionId, moveItemsToSectionId, deletePlacements, cancellationToken)
            .ConfigureAwait(false);

        if (outcome.Outcome == SectionOutcomes.Deleted)
        {
            await NotifyAsync(venueId, "section-deleted", menuId, cancellationToken).ConfigureAwait(false);
        }

        return outcome;
    }

    public async Task<ReorderOutcome> ReorderSectionsAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        CancellationToken cancellationToken = default)
    {
        var outcome = await library.ReorderSectionsGuardedAsync(
            venueId, menuId, sectionIds,
            timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);

        if (outcome.Outcome == ReorderOutcomes.Reordered)
        {
            await NotifyAsync(venueId, "sections-reordered", menuId, cancellationToken).ConfigureAwait(false);
        }

        return outcome;
    }

    public async Task<ReorderOutcome> ReorderItemsAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken = default)
    {
        var outcome = await library.ReorderPlacementsGuardedAsync(
            venueId, menuId, sectionId, itemIds,
            timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);

        if (outcome.Outcome == ReorderOutcomes.Reordered)
        {
            await NotifyAsync(venueId, "items-reordered", menuId, cancellationToken).ConfigureAwait(false);
        }

        return outcome;
    }

    /// <summary>
    /// "Create '&lt;typed&gt;' as a new item": born with exactly the typed name, an
    /// empty price and description, placed in the section (Q113). A missing price
    /// is a quiet flag on the canvas, never a refusal.
    /// </summary>
    public async Task<ItemPlacementOutcome> AddNewItemAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetResolvedCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var limit = ceilings.TryGetValue(MenuCeilings.ItemsPerMenu, out var configured) ? configured : int.MaxValue;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var outcome = await library.CreateItemOnMenuAsync(
            new Item
            {
                Id = itemId,
                VenueId = venueId,
                Name = NormalizeItemName(name),
                CreatedUtc = now,
                UpdatedUtc = now
            },
            menuId, sectionId, limit, cancellationToken).ConfigureAwait(false);

        if (outcome.Outcome == ItemPlacementOutcomes.Created)
        {
            await NotifyAsync(venueId, "item-added", menuId, cancellationToken).ConfigureAwait(false);
        }

        return outcome;
    }

    public async Task<PlaceExistingOutcome> PlaceExistingItemAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetResolvedCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var limit = ceilings.TryGetValue(MenuCeilings.ItemsPerMenu, out var configured) ? configured : int.MaxValue;

        var outcome = await library.PlaceExistingItemAsync(
            venueId, menuId, sectionId, itemId, limit,
            timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);

        if (outcome.Outcome == PlaceExistingOutcomes.Placed)
        {
            await NotifyAsync(venueId, "item-placed", menuId, cancellationToken).ConfigureAwait(false);
        }

        return outcome;
    }

    public async Task<bool> RemoveItemFromMenuAsync(
        Guid venueId,
        Guid menuId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var removed = await library
            .RemoveItemFromMenuAsync(venueId, menuId, itemId, cancellationToken)
            .ConfigureAwait(false);

        if (removed)
        {
            await NotifyAsync(venueId, "item-removed", menuId, cancellationToken).ConfigureAwait(false);
        }

        return removed;
    }

    /// <summary>
    /// Edits an item's values. One item is one shared price across every board it
    /// sits on (Q5), so this reaches all of them — and each board's own screens
    /// still change only when that board publishes.
    /// </summary>
    public async Task<ItemEditResult> UpdateItemValuesAsync(
        Guid venueId,
        Guid itemId,
        string name,
        string? description,
        string? price,
        ItemValueExpectation? expected = null,
        CancellationToken cancellationToken = default)
    {
        var item = await library.GetItemAsync(venueId, itemId, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return new ItemEditResult("not_found", null);
        }

        // An emptied name reverts rather than saving blank (Q119): the schema
        // refuses it anyway, and a refusal a person cannot see is a lost edit.
        item.Name = NormalizeItemName(name, fallback: item.Name);
        item.Description = Trim(description, 1000);
        item.Price = Trim(price, 40);
        item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;

        /*
         * The expectation is normalised the same way the new values are, because it
         * is the same shape of data arriving by the same door. Otherwise an Undo
         * echoing back a description it was handed as "" would be compared against
         * the NULL that was stored, and refuse itself.
         */
        var guard = expected is null
            ? null
            : new ItemValueExpectation(
                NormalizeItemName(expected.Name, fallback: expected.Name),
                Trim(expected.Description, 1000),
                Trim(expected.Price, 40));

        var outcome = await library
            .UpdateItemValuesGuardedAsync(
                venueId,
                itemId,
                item.Name,
                item.Description,
                item.Price,
                guard,
                item.UpdatedUtc,
                cancellationToken)
            .ConfigureAwait(false);

        if (outcome.Outcome != "updated")
        {
            // The values now in place travel with the refusal, so the surface can
            // say what it found rather than only that it declined.
            return new ItemEditResult(
                outcome.Outcome,
                outcome.Outcome == "item_changed"
                    ? new Item
                    {
                        Id = itemId,
                        VenueId = venueId,
                        Name = outcome.Name ?? string.Empty,
                        Description = outcome.Description,
                        Price = outcome.Price
                    }
                    : null);
        }

        await NotifyAsync(venueId, "item-updated", null, cancellationToken).ConfigureAwait(false);
        return new ItemEditResult("updated", item);
    }

    /// <summary>
    /// The add row's search, with the boards each result already sits on so the UI
    /// can label them and jump rather than duplicate (Q112/Q123).
    /// </summary>
    public async Task<IReadOnlyList<LibraryItemResult>> SearchLibraryAsync(
        Guid venueId,
        string? query,
        int take,
        CancellationToken cancellationToken = default)
    {
        var items = await library.SearchItemsAsync(venueId, query, take, cancellationToken).ConfigureAwait(false);
        if (items.Count == 0)
        {
            return [];
        }

        var ids = items.Select(item => item.Id).ToArray();
        var boards = await library.GetItemBoardsAsync(venueId, ids, cancellationToken).ConfigureAwait(false);
        var availability = await library.GetAvailabilityAsync(venueId, cancellationToken).ConfigureAwait(false);
        var offById = availability
            .Where(state => !state.IsAvailable)
            .Select(state => state.ItemId)
            .ToHashSet();

        var boardsByItem = boards
            .GroupBy(board => board.ItemId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<ItemBoard>)[.. group]);

        return
        [
            .. items.Select(item => new LibraryItemResult(
                item,
                !offById.Contains(item.Id),
                boardsByItem.TryGetValue(item.Id, out var on) ? on : []))
        ];
    }

    /// <summary>
    /// The menu themes this venue has. There are none, and there is no table for
    /// them: menu themes are created in the theme editor, which does not exist yet,
    /// and no named looks ship (Q86). The picker renders the empty state from this
    /// rather than from a hard-coded list, so it needs no change when the first
    /// theme is built.
    /// </summary>
    public static IReadOnlyList<MenuThemeResult> GetMenuThemes() => [];

    private static string NormalizeSectionName(string? name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        return trimmed.Length == 0
            ? throw new ArgumentException("A section needs a name.", nameof(name))
            : trimmed[..Math.Min(trimmed.Length, 200)];
    }

    private static string NormalizeItemName(string? name, string? fallback = null)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return fallback ?? throw new ArgumentException("An item needs a name.", nameof(name));
        }

        return trimmed[..Math.Min(trimmed.Length, 200)];
    }

    private static string? Trim(string? value, int max)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed[..Math.Min(trimmed.Length, max)];
    }

    private Task NotifyAsync(Guid venueId, string change, Guid? menuId, CancellationToken cancellationToken) =>
        notifier.NotifyVenueContentUpdatedAsync(venueId, new { change, menuId }, cancellationToken);

    /// <summary>
    /// The ceilings that apply to this venue, always read from the allowance
    /// model, plus the venue timezone every surface renders its times in.
    /// </summary>
    public async Task<MenuContext> GetContextAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetResolvedCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var venue = await venues.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var menuCount = await library.CountMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        return new MenuContext(venue?.Timezone ?? "UTC", ceilings, menuCount);
    }

    /// <summary>
    /// Refuses with a plain sentence rather than failing quietly when a ceiling
    /// is reached. Returns null when there is room.
    /// </summary>
    public async Task<string?> DescribeCeilingRefusalAsync(
        Guid venueId,
        string capabilityId,
        int proposedTotal,
        CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetResolvedCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (!ceilings.TryGetValue(capabilityId, out var limit) || proposedTotal <= limit)
        {
            return null;
        }

        return MenuCeilings.DescribeRefusal(capabilityId, proposedTotal, limit);
    }
}

public sealed record AvailabilityResult(Item Item, ItemAvailability Availability, IReadOnlyCollection<Guid> ScreenIds);

public sealed record RestoreResult(IReadOnlyList<SnapshotChange> Draft, int ReplacedChangeCount);

/// <summary>A tier ceiling refused the act, in the plain words the operator sees (Q201).</summary>
public sealed class MenuCeilingReachedException(string message) : InvalidOperationException(message);

/// <summary>A menu on a screen is taken off deliberately before it can be put away (Q195).</summary>
public sealed class MenuStillOnScreensException(string message) : InvalidOperationException(message);

public sealed record PutAwayResult(bool Changed, int ActiveMenuCount);

/// <summary>
/// One shelf card. <paramref name="Draft"/> is the difference between the board the
/// screens are showing and the board as it stands; <paramref name="PublishedBoard"/>
/// is the first of those two, and is null when the menu has never been published —
/// a state the shelf renders rather than an error. <paramref name="ScreenIds"/> is
/// published truth, never the assignments.
/// </summary>
public sealed record ShelfMenuResult(
    Guid MenuId,
    string Name,
    string? Theme,
    bool IsPutAway,
    long? PublishedVersion,
    DateTime? LastPublishedUtc,
    string? LastPublishedBy,
    IReadOnlyList<SnapshotChange> Draft,
    IReadOnlyCollection<Guid> ScreenIds,
    MenuSnapshot? PublishedBoard);

public sealed record PublishedBoardResult(
    Guid MenuId,
    long Version,
    DateTime PublishedUtc,
    string? Author,
    MenuSnapshot? Board);

/// <summary>The copy's id, the name it actually got, and the venue's active-menu count.</summary>
public sealed record DuplicateResult(Guid MenuId, string Name, int ActiveMenuCount);

/// <summary>
/// ConflictedScreenIds are screens this publish deliberately left alone because
/// another menu has since been given them. Reporting them is what keeps a safe
/// no-op from being a silent one.
/// </summary>
public sealed record PublishResult(
    MenuPublishEvent Event,
    int ChangeCount,
    IReadOnlyCollection<MenuPublishTarget> Targets,
    IReadOnlyCollection<Guid> ConflictedScreenIds);

public sealed record MenuContext(string Timezone, IReadOnlyDictionary<string, int> Ceilings, int MenuCount);

/// <summary>
/// A menu open in the builder. Board is the WORKING state — what the canvas draws,
/// because the canvas is the preview — and Draft is its difference from what the
/// screens are showing.
/// </summary>
public sealed record BuilderBoardResult(
    MenuSnapshot Board,
    IReadOnlyList<SnapshotChange> Draft,
    long? PublishedVersion,
    DateTime? LastPublishedUtc,
    string? LastPublishedBy);

/// <summary>
/// One search result on the add row: the item, whether it is on right now, and the
/// boards it already sits on.
/// </summary>
public sealed record LibraryItemResult(Item Item, bool IsAvailable, IReadOnlyList<ItemBoard> Boards);

/// <summary>
/// An edit's outcome: <c>updated</c>, <c>item_changed</c> (somebody else got there
/// first, and <see cref="Item"/> carries what is there now), or <c>not_found</c>.
/// </summary>
public sealed record ItemEditResult(string Outcome, Item? Item);

/// <summary>A menu theme the venue could attach. There are none yet (Q86).</summary>
public sealed record MenuThemeResult(string Key, string Name);

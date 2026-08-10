using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

/// <summary>
/// Persistence for menu content: the venue item library, placements onto
/// boards, availability, menu-to-screen assignment, the per-menu draft queue,
/// publishes and the attributable history.
/// </summary>
public interface IContentRepository
{
    // ----- Library and placements -------------------------------------------------

    Task<Guid> CreateItemAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an item's values inside its own venue only: the venue id is part
    /// of the WHERE clause, never assumed from the primary key.
    /// </summary>
    Task<bool> UpdateItemAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a menu unless the venue is already at its active-menu ceiling.
    /// The count and the insert commit under one lock, so two requests at
    /// limit-minus-one cannot both succeed (Q201).
    /// </summary>
    Task<MenuCreateOutcome> CreateMenuWithinCeilingAsync(
        Menu menu,
        int activeMenuLimit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a library item and places it on a section in one transaction,
    /// refusing atomically when the menu is at its items ceiling (Q201) or the
    /// section does not sit on this menu in this venue.
    /// </summary>
    Task<ItemPlacementOutcome> CreateItemOnMenuAsync(
        Item item,
        Guid menuId,
        Guid sectionId,
        int itemsPerMenuLimit,
        CancellationToken cancellationToken = default);

    /// <summary>Rewrites a section's placement order; position in the list is the sort order.</summary>
    Task<int> ReorderPlacementsAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime updatedUtc,
        CancellationToken cancellationToken = default);

    /// <summary>Every placement in the venue with its item values and live availability, in board order.</summary>
    /// <summary>
    /// Adds a section at the end of the menu, its sort order read under the same
    /// lock as the insert. Outcomes: <c>created</c>, <c>menu_missing</c>.
    /// </summary>
    Task<SectionCreateOutcome> CreateSectionOnMenuAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<bool> RenameSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a section and releases its items back to the library (Q96), saying
    /// how many were released. Outcomes: <c>deleted</c>, <c>section_missing</c>.
    /// </summary>
    Task<SectionDeleteOutcome> DeleteSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders under a lock that proves the caller's list is still exactly what
    /// the menu holds. Outcomes: <c>reordered</c>, <c>order_stale</c>.
    /// </summary>
    Task<ReorderOutcome> ReorderSectionsGuardedAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits an item's values, optionally only while they are still the values the
    /// caller last saw. Outcomes: <c>updated</c>, <c>item_changed</c>, <c>not_found</c>.
    ///
    /// The guard is what makes Undo safe. An inverse write with no condition is a
    /// blind overwrite: it restores a value from before somebody else's edit and
    /// erases work nobody was told about. With the expectation supplied, Undo means
    /// "put back what I changed, provided what I changed is still what is there."
    /// </summary>
    Task<ItemUpdateOutcome> UpdateItemValuesGuardedAsync(
        Guid venueId,
        Guid itemId,
        string name,
        string? description,
        string? price,
        ItemValueExpectation? expected,
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReorderSectionsGuardedAsync"/>
    Task<ReorderOutcome> ReorderPlacementsGuardedAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Places an item the library already holds. Outcomes: <c>placed</c>,
    /// <c>already_on_board</c> (with the section it sits in, so the UI can jump
    /// rather than duplicate — Q112), <c>ceiling_reached</c>, <c>section_missing</c>,
    /// <c>item_missing</c>.
    /// </summary>
    Task<PlaceExistingOutcome> PlaceExistingItemAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        int itemsPerMenuLimit,
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <summary>Takes an item off one board. The item stays in the library (Q97).</summary>
    Task<bool> RemoveItemFromMenuAsync(
        Guid venueId,
        Guid menuId,
        Guid itemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The add row's search over the whole venue library, 86'd items included
    /// (Q112). Bounded, and wildcards typed by a person are matched literally.
    /// </summary>
    Task<IReadOnlyCollection<Item>> SearchItemsAsync(
        Guid venueId,
        string? query,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>Which boards these items sit on, for "also on Late Night" (Q123).</summary>
    Task<IReadOnlyCollection<ItemBoard>> GetItemBoardsAsync(
        Guid venueId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PlacedMenuItem>> GetPlacedItemsForVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<Item?> GetItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Item>> GetItemsAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<int> CountItemsOnMenuAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    Task<Guid> CreatePlacementAsync(Placement placement, CancellationToken cancellationToken = default);

    Task<bool> RemovePlacementAsync(Guid venueId, Guid placementId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Placement>> GetPlacementsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>Every board this item sits on, across all of the venue's menus.</summary>
    Task<IReadOnlyCollection<Placement>> GetPlacementsForItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default);

    // ----- Availability (86) ------------------------------------------------------

    Task<ItemAvailability> SetAvailabilityAsync(ItemAvailability availability, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ItemAvailability>> GetAvailabilityAsync(Guid venueId, CancellationToken cancellationToken = default);

    // ----- Menu to screen assignment ----------------------------------------------

    Task<MenuScreenAssignment> AssignScreenAsync(MenuScreenAssignment assignment, CancellationToken cancellationToken = default);

    Task<bool> ClearScreenAssignmentAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);

    Task<int> ClearMenuAssignmentsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Takes the menu off its screens in the working state and records the act
    /// with its author in the same transaction (Q68, Q207). The screens keep
    /// showing the published snapshot until the next publish carries it.
    /// </summary>
    Task<int> TakeOffScreensAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Puts a menu away, or back on the shelf, recording the act with its author
    /// in the same transaction. Putting one back is bounded by the same ceiling as
    /// creating one, checked under the same lock; a menu still on a screen is
    /// never put away underneath the person.
    /// </summary>
    Task<PutAwayOutcome> SetPutAwayAsync(
        Guid venueId,
        Guid menuId,
        bool isPutAway,
        int activeMenuLimit,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuScreenAssignment>> GetAssignmentsAsync(Guid venueId, CancellationToken cancellationToken = default);

    /// <summary>
    /// What each screen in the venue is showing right now, taken from what was
    /// published to it - never from the assignments, which are unpublished intent.
    /// </summary>
    Task<IReadOnlyCollection<ScreenShowing>> GetScreensShowingAsync(Guid venueId, CancellationToken cancellationToken = default);

    // ----- Publish and history -----------------------------------------------------

    /// <summary>
    /// Ships the whole queued set in one transaction: publish event, per-target
    /// delivery rows, history entry, cleared queue and the menu's published
    /// version all land together, or nothing does.
    /// </summary>
    /// <param name="changeCount">How many differences this publish is shipping, from the computed draft.</param>
    /// <param name="shippedChanges">Those differences, recorded so history can say what went out.</param>
    /// <param name="expectedWorkingSnapshot">
    /// The working snapshot the shipped set was computed from. The statement
    /// rebuilds it under lock and refuses if the menu has moved, so what history
    /// records always describes the snapshot that went out.
    /// </param>
    /// <param name="expectedPublishedSnapshot">
    /// The published snapshot the shipped set was computed against. Proving the
    /// version alone would still accept a diff taken from another version's
    /// content, which is possible whenever the two are read separately.
    /// </param>
    /// <param name="expectedPublishedVersion">
    /// The published version the shipped set was computed against, so a publish
    /// by someone else in between cannot cause this one to re-ship a difference
    /// that has already reached the screens.
    /// </param>
    Task<PublishOutcome> PublishAsync(
        MenuPublishEvent publishEvent,
        string? shippedChanges,
        string expectedWorkingSnapshot,
        string? expectedPublishedSnapshot,
        long expectedPublishedVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Both halves of the derived draft read together, so a publish landing
    /// between two separate reads cannot produce a diff against a version that is
    /// already gone.
    /// </summary>
    Task<DraftSnapshots> GetDraftSnapshotsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuPublishEvent>> GetPublishHistoryAsync(
        Guid venueId,
        Guid menuId,
        int limit,
        CancellationToken cancellationToken = default);

    Task<MenuPublishEvent?> GetPublishEventAsync(Guid venueId, Guid menuId, long version, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuPublishTarget>> GetPublishTargetsAsync(Guid publishEventId, CancellationToken cancellationToken = default);

    Task<Guid> RecordHistoryAsync(MenuHistoryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>The snapshot the screens are currently showing, or null if never published.</summary>
    Task<string?> GetLatestPublishedSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The board the screens are showing, together with the version, time and author
    /// that put it there — all from one row. Null when the menu has never been
    /// published. Reading the snapshot and then its version separately is the torn
    /// read this model has produced before.
    /// </summary>
    Task<PublishedBoard?> GetLatestPublishedBoardAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every menu the venue has, each with the board its screens are showing and the
    /// board as it stands, in one round trip. The caller derives the draft count from
    /// the pair, so a card can never describe a different board from its own count.
    /// </summary>
    Task<IReadOnlyCollection<ShelfMenu>> GetShelfAsync(Guid venueId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a menu's working state onto a new menu, unless the venue is already at
    /// its active-menu ceiling — counted under the same lock as the insert, because a
    /// duplicate is a new menu and must not be a way around the limit (Q201).
    ///
    /// The copy places the SAME library items (Q20): sharing is the point of a
    /// library, so a later price edit reaches both boards. It is never published and
    /// on no screen, and its name is chosen inside the lock, so two people
    /// duplicating at once cannot both claim it.
    /// </summary>
    Task<MenuDuplicateOutcome> DuplicateMenuWithinCeilingAsync(
        Guid venueId,
        Guid sourceMenuId,
        Guid newMenuId,
        int activeMenuLimit,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default);

    /// <summary>The menu as it stands right now, in the shape a publish records.</summary>
    Task<string?> GetWorkingSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Puts the menu's working state back to a stored snapshot and records the act,
    /// in one transaction. Used by both "go back to" and discard, which are the
    /// same operation against different snapshots. Item identity is preserved: this
    /// restores values onto existing items and never mints new ones (Q43). Refused
    /// for a put-away menu, because a restore puts screen assignments back and
    /// would otherwise be a way onto the shelf around the ceiling and the record.
    /// </summary>
    Task RestoreSnapshotAsync(
        Guid venueId,
        Guid menuId,
        string snapshotJson,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default,
        string kind = MenuHistoryKinds.Restored);

    Task<IReadOnlyCollection<MenuHistoryEntry>> GetHistoryAsync(
        Guid venueId,
        Guid menuId,
        int limit,
        CancellationToken cancellationToken = default);

    // ----- Ceilings ----------------------------------------------------------------

    /// <summary>
    /// The venue's configured ceilings, keyed by capability id. Every ceiling is
    /// read from the allowance model so a tier can change it; the caller never
    /// hard-codes a number.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> GetCeilingsAsync(Guid venueId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The venue's ceilings with the documented defaults filled in for any
    /// capability that has no allowance row. A venue created after the migration
    /// is bounded like every other venue rather than treated as unlimited.
    /// </summary>
    async Task<IReadOnlyDictionary<string, int>> GetResolvedCeilingsAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var configured = await GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var resolved = new Dictionary<string, int>(MenuCeilings.Defaults, StringComparer.Ordinal);
        foreach (var (capabilityId, limit) in configured)
        {
            resolved[capabilityId] = limit;
        }

        return resolved;
    }

    /// <summary>Active menus only: put-away menus do not count against the ceiling.</summary>
    Task<int> CountMenusAsync(Guid venueId, CancellationToken cancellationToken = default);
}

/// <summary>Whether the menu was created, and the venue's active-menu count under the lock.</summary>
public sealed record MenuCreateOutcome(bool Created, int ActiveMenuCount);

/// <summary>
/// Whether the copy was made, the venue's active-menu count under the lock, and the
/// name the copy actually got. The name is returned rather than assumed: the caller
/// asked for "&lt;Name&gt; copy" and may have been given "&lt;Name&gt; copy 3", and telling it
/// so is the difference between the UI showing the truth and showing a guess.
/// </summary>
public sealed record MenuDuplicateOutcome(bool Created, int ActiveMenuCount, string? Name);

/// <summary>
/// A published board and the publish that put it there, read as one row. Author is
/// whoever published it; a null author means the publish recorded none.
/// </summary>
public sealed record PublishedBoard(string? Snapshot, long Version, DateTime PublishedUtc, string? Author);

/// <summary>
/// One menu as the shelf needs it: what it is, what its screens are showing, and what
/// it looks like now. The two snapshots come back together so the difference between
/// them — the card's "N changes not published" — describes these two boards and no
/// others.
/// </summary>
public sealed record ShelfMenu(
    Guid MenuId,
    string Name,
    string? Theme,
    bool IsPutAway,
    long? PublishedVersion,
    DateTime? LastPublishedUtc,
    string? LastPublishedBy,
    string? PublishedSnapshot,
    string? WorkingSnapshot);

/// <summary>
/// Too many copies of one menu to name another. Bounded rather than looping forever,
/// and named rather than failing on a constraint nobody would recognise.
/// </summary>
public sealed class TooManyMenuCopiesException(string message) : InvalidOperationException(message);

/// <summary>
/// The published event, plus any screens this publish deliberately did not touch
/// because another menu has since been given them. Naming them is the difference
/// between a safe no-op and a silent one.
/// </summary>
public sealed record PublishOutcome(MenuPublishEvent Event, IReadOnlyCollection<Guid> ConflictedScreenIds);

/// <summary>
/// The two snapshots the draft is derived from, with the version the published
/// one came from. Publish proves both halves are still current before recording
/// a difference against them.
/// </summary>
/// <summary>
/// One screen and the published version it is showing. Every field but the screen is
/// null when the screen is showing nothing - it was never published to, or the publish
/// that last spoke to it was the one taking a menu off.
/// </summary>
public sealed record ScreenShowing(
    Guid ScreenId,
    string ScreenName,
    Guid? MenuId,
    string? MenuName,
    long? Version,
    DateTime? PublishedUtc,
    string? Author);

public static class SectionOutcomes
{
    public const string Created = "created";
    public const string Deleted = "deleted";
    public const string MenuMissing = "menu_missing";
    public const string SectionMissing = "section_missing";
}

public static class ReorderOutcomes
{
    public const string Reordered = "reordered";

    /// <summary>
    /// The list the caller sent is no longer exactly what the menu holds — someone
    /// added or removed something in between. Refused rather than applied to the
    /// part that still matches, which would leave the rest at stale sort orders.
    /// </summary>
    public const string OrderStale = "order_stale";
}

public static class PlaceExistingOutcomes
{
    public const string Placed = "placed";
    public const string AlreadyOnBoard = "already_on_board";
    public const string CeilingReached = "ceiling_reached";
    public const string SectionMissing = "section_missing";
    public const string ItemMissing = "item_missing";
}

public sealed record SectionCreateOutcome(string Outcome, int SortOrder);

public sealed record SectionDeleteOutcome(string Outcome, int ReleasedItemCount);

public sealed record ReorderOutcome(string Outcome, int Moved);

/// <summary>
/// The values a caller believes an item still holds. Compared under the same lock
/// that writes, because comparing them in a read beforehand proves nothing about
/// the moment of the write.
/// </summary>
public sealed record ItemValueExpectation(string Name, string? Description, string? Price);

/// <summary>
/// The result of a guarded edit, carrying the values now in place when it refused
/// — so the surface can say what it found rather than only that it gave up.
/// </summary>
public sealed record ItemUpdateOutcome(string Outcome, string? Name, string? Description, string? Price);

/// <summary>
/// ExistingSectionId is set only for <c>already_on_board</c>: it is where the item
/// already sits, which is what the UI needs to jump there instead of placing a
/// second copy.
/// </summary>
public sealed record PlaceExistingOutcome(
    string Outcome,
    int ItemCountOnMenu,
    int SortOrder,
    Guid? ExistingSectionId);

/// <summary>One board an item sits on, named rather than counted.</summary>
public sealed record ItemBoard(Guid ItemId, Guid MenuId, string MenuName);

public sealed record DraftSnapshots(
    string? Published,
    string? Working,
    long PublishedVersion,
    DateTime? PublishedUtc,
    string? PublishedBy);

/// <summary>What happened to a put-away or put-back; see <see cref="PutAwayOutcomes"/>.</summary>
public sealed record PutAwayOutcome(string Outcome, int ActiveMenuCount);

/// <summary>
/// Publishing a menu that is on no screen, and has none to release, is refused
/// rather than silently versioning nothing (Q80).
/// </summary>
public sealed class MenuNotOnAnyScreenException(string message) : InvalidOperationException(message);

/// <summary>
/// Every screen involved now shows a different menu. Publishing would reach
/// nothing, and restoring cannot put the menu back to how it looked, so both say
/// so rather than reporting a success that did not happen.
/// </summary>
public sealed class ScreensTakenByAnotherMenuException(string message) : InvalidOperationException(message);

/// <summary>
/// The menu moved between the caller computing its diff and the publish
/// committing, so recording that diff as shipped would be untrue. The caller
/// recomputes and tries again.
/// </summary>
public sealed class MenuMovedWhilePublishingException(string message) : InvalidOperationException(message);

/// <summary>
/// A put-away menu is off the shelf: it cannot be given a screen and cannot be
/// published. Putting one back is its own deliberate, ceiling-checked act, so
/// nothing else may quietly perform that transition.
/// </summary>
public sealed class MenuPutAwayException(string message) : InvalidOperationException(message);

public static class PutAwayOutcomes
{
    public const string Changed = "changed";

    public const string Unchanged = "unchanged";

    public const string NotFound = "not_found";

    public const string OverCeiling = "over_ceiling";

    public const string StillOnScreens = "still_on_screens";
}

/// <summary>
/// What happened to a create-and-place: <see cref="ItemPlacementOutcomes"/> names
/// the cases, and the count is the menu's item total read under the same lock.
/// </summary>
public sealed record ItemPlacementOutcome(string Outcome, int ItemCountOnMenu, int SortOrder);

public static class ItemPlacementOutcomes
{
    public const string Created = "created";

    public const string SectionMissing = "section_missing";

    public const string OverCeiling = "over_ceiling";
}

public static class MenuCeilings
{
    /// <summary>
    /// The ceiling used when a venue has no allowance row of its own - a venue
    /// created after the migration, for instance. A missing row means "not
    /// configured yet", never "unlimited".
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> Defaults =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [MenusPerVenue] = 50,
            [ItemsPerMenu] = 500,
            [ImportLines] = 2000,
            [HistoryRetention] = 50
        };

    /// <summary>
    /// A ceiling refusal in plain words. It names the number, the limit and a way
    /// forward, because a limit that fails quietly is worse than one that explains
    /// itself (Q201).
    /// </summary>
    public static string DescribeRefusal(string capabilityId, int proposedTotal, int limit) => capabilityId switch
    {
        MenusPerVenue => $"That would be {proposedTotal} menus, and this venue is set up for {limit}. Put one away first, or ask us to raise the limit.",
        ItemsPerMenu => $"That would be {proposedTotal} items on one menu, and this venue is set up for {limit}. Split it into two menus.",
        ImportLines => $"That paste is too big - {proposedTotal} lines against a limit of {limit}. Split it into two menus.",
        _ => $"That would be {proposedTotal}, and this venue is set up for {limit}."
    };

    public const string MenusPerVenue = "content.menu.count";

    public const string ItemsPerMenu = "content.menu.items";

    public const string ImportLines = "content.menu.import.lines";

    public const string HistoryRetention = "publishing.history.retention";
}

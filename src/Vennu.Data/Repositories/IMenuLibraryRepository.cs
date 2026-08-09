using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

/// <summary>
/// Persistence for the Menus spine: the venue item library, placements onto
/// boards, availability, menu-to-screen assignment, the per-menu draft queue,
/// publishes and the attributable history.
/// </summary>
public interface IMenuLibraryRepository
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
    /// rebuilds it under lock and refuses (SQL error 51003) if the menu has moved,
    /// so what history records always describes the snapshot that went out.
    /// </param>
    Task<PublishOutcome> PublishAsync(
        MenuPublishEvent publishEvent,
        int changeCount,
        string? shippedChanges,
        string expectedWorkingSnapshot,
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

    /// <summary>The menu as it stands right now, in the shape a publish records.</summary>
    Task<string?> GetWorkingSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Puts the menu's working state back to a stored snapshot and records the act,
    /// in one transaction. Used by both "go back to" and discard, which are the
    /// same operation against different snapshots. Item identity is preserved: this
    /// restores values onto existing items and never mints new ones (Q43).
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
/// The published event, plus any screens this publish deliberately did not touch
/// because another menu has since been given them. Naming them is the difference
/// between a safe no-op and a silent one.
/// </summary>
public sealed record PublishOutcome(MenuPublishEvent Event, IReadOnlyCollection<Guid> ConflictedScreenIds);

/// <summary>The two snapshots the draft is derived from, read together.</summary>
public sealed record DraftSnapshots(string? Published, string? Working);

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

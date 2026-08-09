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

    Task<bool> UpdateItemAsync(Item item, CancellationToken cancellationToken = default);

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

    Task<IReadOnlyCollection<MenuScreenAssignment>> GetAssignmentsAsync(Guid venueId, CancellationToken cancellationToken = default);

    // ----- Draft queue -------------------------------------------------------------

    /// <summary>
    /// Records a change against the menu's draft. Editing the same field again
    /// replaces the existing row, and taking a value back to what is published
    /// removes it, so the queue is always the current diff (Q182). Returns null
    /// when the change collapsed and nothing remains queued for that field.
    /// </summary>
    Task<MenuDraftChange?> UpsertDraftChangeAsync(MenuDraftChange change, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuDraftChange>> GetDraftChangesAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Empties the menu's queue. When <paramref name="recordHistory"/> is set the
    /// clearing and its attributable history entry commit together, so a discard
    /// can never happen anonymously.
    /// </summary>
    Task<int> ClearDraftAsync(
        Guid venueId,
        Guid menuId,
        string? author = null,
        bool recordHistory = false,
        CancellationToken cancellationToken = default);

    // ----- Publish and history -----------------------------------------------------

    Task<long> GetNextPublishVersionAsync(Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ships the whole queued set in one transaction: publish event, per-target
    /// delivery rows, history entry, cleared queue and the menu's published
    /// version all land together, or nothing does.
    /// </summary>
    Task<MenuPublishEvent> PublishAsync(
        MenuPublishEvent publishEvent,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuPublishEvent>> GetPublishHistoryAsync(
        Guid venueId,
        Guid menuId,
        int limit,
        CancellationToken cancellationToken = default);

    Task<MenuPublishEvent?> GetPublishEventAsync(Guid venueId, Guid menuId, long version, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuPublishTarget>> GetPublishTargetsAsync(Guid publishEventId, CancellationToken cancellationToken = default);

    Task<Guid> RecordHistoryAsync(MenuHistoryEntry entry, CancellationToken cancellationToken = default);

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

    Task<int> CountMenusAsync(Guid venueId, CancellationToken cancellationToken = default);
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

using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Services;

/// <summary>
/// The Menus save model. Two rules govern everything here:
/// availability is a fact that commits instantly, and everything else is an
/// intention that waits in the menu's draft until someone publishes it.
/// </summary>
public sealed class MenuSpineService(
    IMenuLibraryRepository library,
    IVenueRepository venues,
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

        return new AvailabilityResult(item, availability, screenIds);
    }

    /// <summary>
    /// Queues one change against a menu. Re-editing the same field replaces the
    /// queued row, so the count always equals what Publish will ship.
    /// </summary>
    public Task<MenuDraftChange> QueueChangeAsync(
        Guid venueId,
        Guid menuId,
        string targetKind,
        Guid? targetId,
        string field,
        string? beforeValue,
        string? afterValue,
        string? author,
        CancellationToken cancellationToken = default) =>
        library.UpsertDraftChangeAsync(
            new MenuDraftChange
            {
                VenueId = venueId,
                MenuId = menuId,
                TargetKind = targetKind,
                TargetId = targetId,
                Field = field,
                BeforeValue = beforeValue,
                AfterValue = afterValue,
                Author = author,
                CreatedUtc = timeProvider.GetUtcNow().UtcDateTime
            },
            cancellationToken);

    public Task<IReadOnlyCollection<MenuDraftChange>> GetDraftAsync(
        Guid venueId,
        Guid menuId,
        CancellationToken cancellationToken = default) =>
        library.GetDraftChangesAsync(venueId, menuId, cancellationToken);

    /// <summary>
    /// Ships every queued change for this menu, and nothing belonging to another.
    /// Atomic: on failure nothing reaches a screen and the draft is untouched.
    /// </summary>
    public async Task<PublishResult> PublishAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var queued = await library.GetDraftChangesAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var assignments = await library.GetAssignmentsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var targets = assignments
            .Where(assignment => assignment.MenuId == menuId)
            .Select(assignment => assignment.ScreenId)
            .ToArray();

        var published = await library.PublishAsync(
            new MenuPublishEvent
            {
                VenueId = venueId,
                MenuId = menuId,
                Author = author,
                PublishedUtc = timeProvider.GetUtcNow().UtcDateTime
            },
            targets,
            cancellationToken).ConfigureAwait(false);

        var deliveries = await library.GetPublishTargetsAsync(published.Id, cancellationToken).ConfigureAwait(false);
        return new PublishResult(published, queued.Count, deliveries);
    }

    /// <summary>
    /// "Go back to" — phrased as a time, never a version. It produces a draft you
    /// then publish; it is never a second silent path to the screens.
    /// </summary>
    public async Task<IReadOnlyCollection<MenuDraftChange>> GoBackToAsync(
        Guid venueId,
        Guid menuId,
        long version,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var target = await library.GetPublishEventAsync(venueId, menuId, version, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Menu '{menuId}' has no published version {version}.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        await library.UpsertDraftChangeAsync(
            new MenuDraftChange
            {
                VenueId = venueId,
                MenuId = menuId,
                TargetKind = DraftTargetKinds.Menu,
                TargetId = null,
                Field = "restoredFromVersion",
                BeforeValue = null,
                AfterValue = target.Version.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Author = author,
                CreatedUtc = now
            },
            cancellationToken).ConfigureAwait(false);

        await library.RecordHistoryAsync(
            new MenuHistoryEntry
            {
                VenueId = venueId,
                MenuId = menuId,
                Kind = MenuHistoryKinds.Restored,
                Detail = $"Prepared a draft from the version published {target.PublishedUtc:O}.",
                Author = author,
                OccurredUtc = now
            },
            cancellationToken).ConfigureAwait(false);

        return await library.GetDraftChangesAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Throws the draft away. The one irreversible act in the model, so it is
    /// recorded with its author and count rather than happening anonymously.
    /// </summary>
    public async Task<int> DiscardDraftAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var queued = await library.GetDraftChangesAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var cleared = await library.ClearDraftAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);

        await library.RecordHistoryAsync(
            new MenuHistoryEntry
            {
                VenueId = venueId,
                MenuId = menuId,
                Kind = MenuHistoryKinds.DraftDiscarded,
                Detail = $"Discarded {queued.Count} queued change(s).",
                Author = author,
                OccurredUtc = timeProvider.GetUtcNow().UtcDateTime
            },
            cancellationToken).ConfigureAwait(false);

        return cleared;
    }

    public async Task<MenuScreenAssignment> AssignAsync(
        Guid venueId,
        Guid screenId,
        Guid menuId,
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
                AssignedUtc = now,
                AssignedBy = author
            },
            cancellationToken).ConfigureAwait(false);

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
    /// "Take off the screens" — the menu keeps its place and its history; only
    /// its screens are released.
    /// </summary>
    public async Task<int> TakeOffScreensAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var cleared = await library.ClearMenuAssignmentsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);

        await library.RecordHistoryAsync(
            new MenuHistoryEntry
            {
                VenueId = venueId,
                MenuId = menuId,
                Kind = MenuHistoryKinds.TakenOffScreens,
                Detail = $"Released {cleared} screen(s); the venue fallback shows instead.",
                Author = author,
                OccurredUtc = timeProvider.GetUtcNow().UtcDateTime
            },
            cancellationToken).ConfigureAwait(false);

        return cleared;
    }

    /// <summary>
    /// The ceilings that apply to this venue, always read from the allowance
    /// model, plus the venue timezone every surface renders its times in.
    /// </summary>
    public async Task<MenuContext> GetContextAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
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
        var ceilings = await library.GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (!ceilings.TryGetValue(capabilityId, out var limit) || proposedTotal <= limit)
        {
            return null;
        }

        return capabilityId switch
        {
            MenuCeilings.MenusPerVenue => $"That would be {proposedTotal} menus, and this venue is set up for {limit}. Put one away first, or ask us to raise the limit.",
            MenuCeilings.ItemsPerMenu => $"That would be {proposedTotal} items on one menu, and this venue is set up for {limit}. Split it into two menus.",
            MenuCeilings.ImportLines => $"That paste is too big — {proposedTotal} lines against a limit of {limit}. Split it into two menus.",
            _ => $"That would be {proposedTotal}, and this venue is set up for {limit}."
        };
    }
}

public sealed record AvailabilityResult(Item Item, ItemAvailability Availability, IReadOnlyCollection<Guid> ScreenIds);

public sealed record PublishResult(MenuPublishEvent Event, int ChangeCount, IReadOnlyCollection<MenuPublishTarget> Targets);

public sealed record MenuContext(string Timezone, IReadOnlyDictionary<string, int> Ceilings, int MenuCount);

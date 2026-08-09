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

    /// <summary>
    /// Queues one change against a menu. Re-editing the same field replaces the
    /// queued row, so the count always equals what Publish will ship.
    /// </summary>
    public Task<MenuDraftChange?> QueueChangeAsync(
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
    /// <summary>
    /// Ships every queued change for this menu, and nothing belonging to another.
    /// Atomic: on failure nothing reaches a screen and the draft is untouched.
    /// Refused when the menu is on no screen — a publish that reaches nothing is a
    /// named state, not a silent success (Q80).
    /// </summary>
    public async Task<PublishResult> PublishAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var assignments = await library.GetAssignmentsAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (!assignments.Any(assignment => assignment.MenuId == menuId))
        {
            throw new MenuNotOnAnyScreenException(
                "Pair a screen to publish. This menu is not on a screen yet, so publishing it would reach nothing.");
        }

        var published = await library.PublishAsync(
            new MenuPublishEvent
            {
                VenueId = venueId,
                MenuId = menuId,
                Author = author,
                PublishedUtc = timeProvider.GetUtcNow().UtcDateTime
            },
            cancellationToken).ConfigureAwait(false);

        var deliveries = await library.GetPublishTargetsAsync(published.Id, cancellationToken).ConfigureAwait(false);

        // The count comes from the publish itself, captured as the queue was
        // removed, not from a read taken before it.
        return new PublishResult(published, published.ChangeCount, deliveries);
    }

    /// <summary>
    /// "Go back to" — phrased as a time, never a version. It rebuilds the draft
    /// from that version's snapshot and REPLACES whatever was queued (Q67), then
    /// waits for a deliberate publish. It is never a second silent path to the
    /// screens. The count of replaced changes is returned so the caller can warn
    /// before committing.
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

        var replaced = await library.GetDraftChangesAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // The restore replaces the queue rather than stacking on it, so the two
        // cannot disagree about what the menu should become.
        await library.ClearDraftAsync(venueId, menuId, author, recordHistory: false, cancellationToken).ConfigureAwait(false);

        var current = await BuildCurrentSnapshotAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var changes = MenuSnapshot.Diff(current, target.Snapshot!);

        foreach (var change in changes)
        {
            await library.UpsertDraftChangeAsync(
                new MenuDraftChange
                {
                    VenueId = venueId,
                    MenuId = menuId,
                    TargetKind = change.TargetKind,
                    TargetId = change.TargetId,
                    Field = change.Field,
                    BeforeValue = change.BeforeValue,
                    AfterValue = change.AfterValue,
                    Author = author,
                    CreatedUtc = now
                },
                cancellationToken).ConfigureAwait(false);
        }

        await library.RecordHistoryAsync(
            new MenuHistoryEntry
            {
                VenueId = venueId,
                MenuId = menuId,
                Kind = MenuHistoryKinds.Restored,
                Detail = $"Rebuilt the draft from the version published {target.PublishedUtc:O}, replacing {replaced.Count} queued change(s).",
                Author = author,
                OccurredUtc = now
            },
            cancellationToken).ConfigureAwait(false);

        var draft = await library.GetDraftChangesAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        return new RestoreResult(draft, replaced.Count);
    }

    /// <summary>
    /// Throws the draft away. The one irreversible act in the model, so the
    /// clearing and the record naming who did it commit together — a partial
    /// failure cannot leave the work gone and the act anonymous (Q207).
    /// </summary>
    public Task<int> DiscardDraftAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default) =>
        library.ClearDraftAsync(venueId, menuId, author, recordHistory: true, cancellationToken);

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
    /// "Take off the screens" is permanent, so unlike an 86 it queues as a draft
    /// change and reaches the screens through Publish (Q68). The menu keeps its
    /// place and its history; only its screens are released, and only when the
    /// operator deliberately publishes.
    /// </summary>
    public async Task<MenuDraftChange?> QueueTakeOffScreensAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        var assignments = await library.GetAssignmentsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var current = assignments.Where(a => a.MenuId == menuId).Select(a => a.ScreenId).ToArray();

        return await library.UpsertDraftChangeAsync(
            new MenuDraftChange
            {
                VenueId = venueId,
                MenuId = menuId,
                TargetKind = DraftTargetKinds.Screens,
                TargetId = null,
                Field = "assignedScreens",
                BeforeValue = string.Join(",", current.Select(id => id.ToString())),
                AfterValue = string.Empty,
                Author = author,
                CreatedUtc = timeProvider.GetUtcNow().UtcDateTime
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// The menu as it stands right now, in the same shape a publish records, so a
    /// restore can be expressed as the difference between now and then.
    /// </summary>
    private async Task<string?> BuildCurrentSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken)
    {
        var placements = await library.GetPlacementsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var items = (await library.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false))
            .ToDictionary(item => item.Id);

        var snapshot = new MenuSnapshot
        {
            MenuId = menuId,
            Sections =
            [
                .. placements
                    .GroupBy(placement => placement.MenuSectionId)
                    .Select(group => new SnapshotSection
                    {
                        SectionId = group.Key,
                        Items =
                        [
                            .. group
                                .OrderBy(placement => placement.SortOrder)
                                .Where(placement => items.ContainsKey(placement.ItemId))
                                .Select(placement => new SnapshotItem
                                {
                                    ItemId = placement.ItemId,
                                    Name = items[placement.ItemId].Name,
                                    Description = items[placement.ItemId].Description,
                                    Price = items[placement.ItemId].Price,
                                    SortOrder = placement.SortOrder
                                })
                        ]
                    })
            ]
        };

        return MenuSnapshot.Serialize(snapshot);
    }

    /// <summary>
    /// The ceilings that apply to this venue, always read from the allowance
    /// model, plus the venue timezone every surface renders its times in.
    /// </summary>
    public async Task<MenuContext> GetContextAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var ceilings = await ResolveCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var venue = await venues.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var menuCount = await library.CountMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        return new MenuContext(venue?.Timezone ?? "UTC", ceilings, menuCount);
    }

    /// <summary>
    /// The venue's ceilings, falling back to the documented defaults for any
    /// capability with no allowance row. A venue created after the migration is
    /// therefore bounded like every other venue, rather than being treated as
    /// unlimited because nobody provisioned a row for it.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, int>> ResolveCeilingsAsync(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        var configured = await library.GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var resolved = new Dictionary<string, int>(MenuCeilings.Defaults, StringComparer.Ordinal);
        foreach (var (capabilityId, limit) in configured)
        {
            resolved[capabilityId] = limit;
        }

        return resolved;
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
        var ceilings = await ResolveCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
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

public sealed record RestoreResult(IReadOnlyCollection<MenuDraftChange> Draft, int ReplacedChangeCount);

/// <summary>
/// Publishing a menu that is on no screen is refused rather than silently
/// versioning nothing (Q80).
/// </summary>
public sealed class MenuNotOnAnyScreenException(string message) : InvalidOperationException(message);

public sealed record PublishResult(MenuPublishEvent Event, int ChangeCount, IReadOnlyCollection<MenuPublishTarget> Targets);

public sealed record MenuContext(string Timezone, IReadOnlyDictionary<string, int> Ceilings, int MenuCount);

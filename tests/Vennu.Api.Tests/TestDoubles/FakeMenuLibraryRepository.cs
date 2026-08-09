using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for the Menus spine. It deliberately mirrors the two
/// invariants the SQL enforces: the draft queue is keyed by (menu, target,
/// field) so it stays the current diff, and a publish clears only its own
/// menu's queue.
/// </summary>
internal sealed class FakeMenuLibraryRepository : IMenuLibraryRepository
{
    public List<Item> Items { get; } = [];

    public List<Placement> Placements { get; } = [];

    public List<ItemAvailability> Availability { get; } = [];

    public List<MenuScreenAssignment> Assignments { get; } = [];

    public List<MenuDraftChange> DraftChanges { get; } = [];

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

    public Task<IReadOnlyCollection<MenuScreenAssignment>> GetAssignmentsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuScreenAssignment>>(
            Assignments.Where(a => a.VenueId == venueId).ToArray());

    // ----- Draft queue -----

    public Task<MenuDraftChange?> UpsertDraftChangeAsync(MenuDraftChange change, CancellationToken cancellationToken = default)
    {
        // Mirror the statement: a value taken back to what is published is not a
        // change, so it leaves the queue rather than sitting in it.
        if (string.Equals(change.BeforeValue, change.AfterValue, StringComparison.Ordinal))
        {
            DraftChanges.RemoveAll(candidate =>
                candidate.MenuId == change.MenuId
                && string.Equals(candidate.TargetKind, change.TargetKind, StringComparison.Ordinal)
                && candidate.TargetId == change.TargetId
                && string.Equals(candidate.Field, change.Field, StringComparison.Ordinal));
            return Task.FromResult<MenuDraftChange?>(null);
        }

        var existing = DraftChanges.SingleOrDefault(candidate =>
            candidate.MenuId == change.MenuId
            && string.Equals(candidate.TargetKind, change.TargetKind, StringComparison.Ordinal)
            && candidate.TargetId == change.TargetId
            && string.Equals(candidate.Field, change.Field, StringComparison.Ordinal));

        if (existing is not null)
        {
            existing.AfterValue = change.AfterValue;
            existing.Author = change.Author;
            existing.UpdatedUtc = change.CreatedUtc;
            return Task.FromResult<MenuDraftChange?>(existing);
        }

        change.Id = change.Id == Guid.Empty ? Guid.NewGuid() : change.Id;
        change.UpdatedUtc = change.CreatedUtc;
        DraftChanges.Add(change);
        return Task.FromResult<MenuDraftChange?>(change);
    }

    public Task<IReadOnlyCollection<MenuDraftChange>> GetDraftChangesAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<MenuDraftChange>>(
            DraftChanges.Where(change => change.VenueId == venueId && change.MenuId == menuId).ToArray());

    public Task<int> ClearDraftAsync(
        Guid venueId,
        Guid menuId,
        string? author = null,
        bool recordHistory = false,
        CancellationToken cancellationToken = default)
    {
        var removed = DraftChanges.RemoveAll(change => change.VenueId == venueId && change.MenuId == menuId);
        if (removed > 0 && recordHistory)
        {
            History.Add(new MenuHistoryEntry
            {
                Id = Guid.NewGuid(),
                VenueId = venueId,
                MenuId = menuId,
                Kind = MenuHistoryKinds.DraftDiscarded,
                Detail = $"Discarded {removed} queued change(s).",
                Author = author,
                OccurredUtc = DateTime.UtcNow
            });
        }

        return Task.FromResult(removed);
    }

    // ----- Publish and history -----

    public Task<long> GetNextPublishVersionAsync(Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(PublishEvents.Where(e => e.MenuId == menuId).Select(e => e.Version).DefaultIfEmpty(0).Max() + 1);

    public Task<MenuPublishEvent> PublishAsync(
        MenuPublishEvent publishEvent,
        CancellationToken cancellationToken = default)
    {
        publishEvent.Id = publishEvent.Id == Guid.Empty ? Guid.NewGuid() : publishEvent.Id;
        publishEvent.Version = PublishEvents
            .Where(e => e.MenuId == publishEvent.MenuId)
            .Select(e => e.Version)
            .DefaultIfEmpty(0)
            .Max() + 1;

        // Capture the queue and clear it in one step, exactly as the statement
        // does, so the recorded count is what actually shipped.
        var shipped = DraftChanges
            .Where(change => change.VenueId == publishEvent.VenueId && change.MenuId == publishEvent.MenuId)
            .ToList();
        DraftChanges.RemoveAll(change =>
            change.VenueId == publishEvent.VenueId && change.MenuId == publishEvent.MenuId);

        publishEvent.ChangeCount = shipped.Count;
        publishEvent.ShippedChanges = System.Text.Json.JsonSerializer.Serialize(shipped);
        publishEvent.Snapshot ??= SnapshotJson ?? "{\"menuId\":\"" + publishEvent.MenuId + "\",\"sections\":[]}";

        PublishEvents.Add(publishEvent);

        // Targets come from the assignments, never from a caller-supplied list.
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

        return Task.FromResult(publishEvent);
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

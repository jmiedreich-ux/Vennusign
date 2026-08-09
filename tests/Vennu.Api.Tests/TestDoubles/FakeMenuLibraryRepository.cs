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

    public Task<string?> GetWorkingSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(WorkingSnapshotJson);

    public Task<string?> GetLatestPublishedSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        Task.FromResult(PublishEvents
            .Where(e => e.VenueId == venueId && e.MenuId == menuId)
            .OrderByDescending(e => e.Version)
            .Select(e => e.Snapshot)
            .FirstOrDefault());

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
        // The statement puts the working rows back; the fake models that by making
        // the working snapshot the restored one.
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

    public Task<MenuPublishEvent> PublishAsync(
        MenuPublishEvent publishEvent,
        int changeCount,
        string? shippedChanges,
        CancellationToken cancellationToken = default)
    {
        var assigned = Assignments
            .Where(a => a.VenueId == publishEvent.VenueId && a.MenuId == publishEvent.MenuId)
            .Select(a => a.ScreenId)
            .ToList();
        var previouslyTargeted = PublishEvents
            .Where(e => e.MenuId == publishEvent.MenuId)
            .OrderByDescending(e => e.Version)
            .Take(1)
            .SelectMany(e => PublishTargets.Where(t => t.PublishEventId == e.Id).Select(t => t.ScreenId))
            .ToList();

        // Q80 lives in the statement, so the fake mirrors it.
        if (assigned.Count == 0 && previouslyTargeted.Count == 0)
        {
            throw new MenuNotOnAnyScreenException(
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
        publishEvent.Snapshot = WorkingSnapshotJson;

        PublishEvents.Add(publishEvent);

        foreach (var screenId in assigned.Union(previouslyTargeted))
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

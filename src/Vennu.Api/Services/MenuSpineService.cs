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
        CancellationToken cancellationToken = default)
    {
        var working = await library.GetWorkingSnapshotAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        if (working is null)
        {
            throw new InvalidOperationException($"Menu '{menuId}' does not belong to venue '{venueId}'.");
        }

        var published = await library.GetLatestPublishedSnapshotAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        return MenuSnapshot.Diff(published, working);
    }

    /// <summary>
    /// Ships the menu as it stands to its screens. Refused when the menu is on no
    /// screen and has none to release — a publish that reaches nothing is a named
    /// state, not a silent version bump (Q80, enforced inside the transaction).
    /// </summary>
    public async Task<PublishResult> PublishAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        CancellationToken cancellationToken = default)
    {
        // The shipped set is the diff at the moment of publishing, recorded so the
        // history can say what went out. It is descriptive: the snapshot is the
        // authority on what the screens now show.
        var shipped = await GetDraftAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var shippedJson = System.Text.Json.JsonSerializer.Serialize(shipped, JsonOptions);

        MenuPublishEvent published;
        try
        {
            published = await library.PublishAsync(
                new MenuPublishEvent
                {
                    VenueId = venueId,
                    MenuId = menuId,
                    Author = author,
                    PublishedUtc = timeProvider.GetUtcNow().UtcDateTime
                },
                shipped.Count,
                shippedJson,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51002)
        {
            throw new MenuNotOnAnyScreenException(exception.Message);
        }

        var deliveries = await library.GetPublishTargetsAsync(published.Id, cancellationToken).ConfigureAwait(false);
        return new PublishResult(published, published.ChangeCount, deliveries);
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
        await library.RestoreSnapshotAsync(
            venueId,
            menuId,
            target.Snapshot!,
            author,
            $"Put the menu back to how it looked when version {version} was published, replacing {displaced} unpublished change(s).",
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);

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
        await library.ClearMenuAssignmentsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);

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

/// <summary>
/// Publishing a menu that is on no screen is refused rather than silently
/// versioning nothing (Q80).
/// </summary>
public sealed class MenuNotOnAnyScreenException(string message) : InvalidOperationException(message);

public sealed record PublishResult(MenuPublishEvent Event, int ChangeCount, IReadOnlyCollection<MenuPublishTarget> Targets);

public sealed record MenuContext(string Timezone, IReadOnlyDictionary<string, int> Ceilings, int MenuCount);

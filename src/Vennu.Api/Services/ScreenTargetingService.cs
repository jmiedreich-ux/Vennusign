using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Notifications;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Services;

public sealed class ScreenTargetingService(
    IScreenRepository screenRepository,
    IVenueRepository venueRepository,
    IMenuRepository menuRepository,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider,
    IScreenContentDeliveryService? deliveryService = null) : IScreenTargetingService
{
    private static readonly int[] SupportedCapacities = [4, 6, 8, 9];

    public async Task<int> PushAllAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var screens = await screenRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var activeScreens = screens.Where(screen => !string.Equals(screen.Status, "Archived", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (activeScreens.Length == 0)
        {
            return 0;
        }

        foreach (var screen in activeScreens.OrderBy(item => item.Id))
        {
            var delivery = deliveryService is null ? null : await deliveryService.IssueAsync(venueId, screen.Id, cancellationToken).ConfigureAwait(false);
            await notifier.NotifyScreenContentUpdatedAsync(screen.Id, new
            {
                change = "manual-push",
                requestedUtc = delivery?.RequestedUtc ?? timeProvider.GetUtcNow().UtcDateTime,
                revision = delivery?.AuthoritativeRevision
            }, cancellationToken).ConfigureAwait(false);
        }
        return activeScreens.Length;
    }

    public async Task<ScreenOverflowPreview> GetOverflowAsync(
        Guid venueId,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        if (!SupportedCapacities.Contains(capacity))
        {
            throw new ArgumentException("Capacity must be one of 4, 6, 8, or 9.", nameof(capacity));
        }
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);

        var menus = await menuRepository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        var menu = menus
            .Where(candidate => candidate.IsActive)
            .OrderBy(candidate => candidate.CreatedUtc)
            .ThenBy(candidate => candidate.Id)
            .FirstOrDefault();
        if (menu is null)
        {
            return new ScreenOverflowPreview(capacity, 0, 0, 0, []);
        }

        var sections = await menuRepository.GetSectionsAsync(venueId, menu.Id, cancellationToken).ConfigureAwait(false);
        var orderedSections = sections
            .Where(section => section.IsActive)
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Id)
            .ToArray();
        var items = new List<ScreenOverflowItem>();
        foreach (var section in orderedSections)
        {
            var sectionItems = await menuRepository.GetActiveItemsAsync(venueId, section.Id, cancellationToken).ConfigureAwait(false);
            foreach (var item in sectionItems
                .Where(item => item.IsAvailable)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id))
            {
                items.Add(new ScreenOverflowItem(item.Id, section.Name, item.Name, items.Count < capacity));
            }
        }

        var visible = Math.Min(capacity, items.Count);
        return new ScreenOverflowPreview(capacity, items.Count, visible, items.Count - visible, items);
    }

    private async Task RequireVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        if (await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Venue does not exist.");
        }
    }
}

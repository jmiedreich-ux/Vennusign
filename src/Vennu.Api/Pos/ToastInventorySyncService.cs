using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed record ToastInventoryApplyResult(int ItemsExamined, int ItemsUpdated, int NotificationsPublished);

public interface IToastInventorySyncService
{
    Task<ToastInventoryApplyResult> ApplyItemAsync(
        Guid venueId,
        PosInventoryItem inventory,
        CancellationToken cancellationToken = default);

    Task<ToastInventoryApplyResult> ApplySnapshotAsync(
        Guid venueId,
        IReadOnlyCollection<PosInventoryItem> inventory,
        CancellationToken cancellationToken = default);
}

public sealed class ToastInventorySyncService(
    IPosCatalogMappingRepository mappings,
    IMenuRepository menus,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : IToastInventorySyncService
{
    public Task<ToastInventoryApplyResult> ApplyItemAsync(
        Guid venueId,
        PosInventoryItem inventory,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(venueId, [inventory], cancellationToken);

    public Task<ToastInventoryApplyResult> ApplySnapshotAsync(
        Guid venueId,
        IReadOnlyCollection<PosInventoryItem> inventory,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(venueId, inventory, cancellationToken);

    private async Task<ToastInventoryApplyResult> ApplyAsync(
        Guid venueId,
        IReadOnlyCollection<PosInventoryItem> inventory,
        CancellationToken cancellationToken)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("A venue is required.", nameof(venueId));
        ArgumentNullException.ThrowIfNull(inventory);
        var normalized = inventory
            .GroupBy(value => value.ExternalItemId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToArray();
        var updated = 0;
        var notifications = 0;
        foreach (var value in normalized)
        {
            if (string.IsNullOrWhiteSpace(value.ExternalItemId)) continue;
            var item = await mappings.GetMappedItemAsync(
                venueId, PosProvider.Toast, value.ExternalItemId, cancellationToken).ConfigureAwait(false);
            if (item is null) continue;
            var availabilityChanged = item.IsAvailable != value.IsAvailable;
            var quantityChanged = item.QuantityAvailable != value.QuantityAvailable;
            if (!availabilityChanged && !quantityChanged) continue;
            item.IsAvailable = value.IsAvailable;
            item.QuantityAvailable = value.QuantityAvailable;
            item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
            if (!await menus.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false))
                throw new InvalidOperationException("The Toast inventory update could not be persisted.");
            updated++;
            if (availabilityChanged)
            {
                await notifier.NotifyVenueItemAvailabilityChangedAsync(
                    venueId, item.Id.ToString(), value.IsAvailable, cancellationToken).ConfigureAwait(false);
                notifications++;
            }
            if (quantityChanged)
            {
                await notifier.NotifyVenueContentUpdatedAsync(
                    venueId, new { change = "pos-quantity", itemId = item.Id }, cancellationToken).ConfigureAwait(false);
                notifications++;
            }
        }

        return new ToastInventoryApplyResult(normalized.Length, updated, notifications);
    }
}

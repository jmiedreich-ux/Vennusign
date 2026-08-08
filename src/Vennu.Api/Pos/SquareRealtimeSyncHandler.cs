using System.Globalization;
using System.Text.Json;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class SquareRealtimeSyncHandler(
    IPosConnectionRepository connections,
    IPosCatalogMappingRepository mappings,
    IMenuRepository menus,
    IEnumerable<IPosProvider> providers,
    IPosCredentialProtector credentialProtector,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : IPosWebhookEventHandler
{
    private const string InventoryEvent = "inventory.count.updated";
    private const string CatalogEvent = "catalog.version.updated";

    public bool CanHandle(PosProvider provider, string eventType) =>
        provider == PosProvider.Square && eventType is InventoryEvent or CatalogEvent;

    public async Task HandleAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        if (!CanHandle(webhookEvent.Provider, webhookEvent.EventType)) return;
        var connection = await connections.GetByExternalMerchantIdAsync(
            PosProvider.Square, webhookEvent.ExternalMerchantId, cancellationToken).ConfigureAwait(false);
        if (connection is null || connection.Status != PosConnectionStatus.Connected) return;

        if (webhookEvent.EventType == InventoryEvent)
            await ApplyInventoryAsync(connection, webhookEvent.Payload, cancellationToken).ConfigureAwait(false);
        else
            await ApplyPricesAsync(connection, cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyInventoryAsync(PosConnection connection, string payload, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(payload, new JsonDocumentOptions { MaxDepth = 32 });
        if (!TryInventoryCounts(document.RootElement, out var counts)) return;
        var inStock = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var count in counts.EnumerateArray())
        {
            if (!TryString(count, "catalog_object_id", out var externalId) ||
                !TryString(count, "state", out var state) || state != "IN_STOCK" ||
                !TryString(count, "quantity", out var value) ||
                !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity) || quantity < 0)
                continue;
            inStock[externalId] = inStock.GetValueOrDefault(externalId) + quantity;
        }

        foreach (var (externalId, value) in inStock.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            var item = await mappings.GetMappedItemAsync(connection.VenueId, PosProvider.Square, externalId, cancellationToken).ConfigureAwait(false);
            if (item is null) continue;
            var quantity = value <= int.MaxValue && decimal.Truncate(value) == value ? (int)value : (int?)null;
            var available = value > 0;
            var availabilityChanged = item.IsAvailable != available;
            var quantityChanged = item.QuantityAvailable != quantity;
            if (!availabilityChanged && !quantityChanged) continue;
            item.IsAvailable = available;
            item.QuantityAvailable = quantity;
            item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
            if (!await menus.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false))
                throw new InvalidOperationException("The Square inventory update could not be persisted.");
            if (availabilityChanged)
                await notifier.NotifyVenueItemAvailabilityChangedAsync(connection.VenueId, item.Id.ToString(), available, cancellationToken).ConfigureAwait(false);
            if (quantityChanged)
                await notifier.NotifyVenueContentUpdatedAsync(connection.VenueId, new { change = "pos-quantity", itemId = item.Id }, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ApplyPricesAsync(PosConnection connection, CancellationToken cancellationToken)
    {
        var provider = providers.Single(value => value.Provider == PosProvider.Square);
        var catalog = await provider.GetCatalogAsync(new PosProviderContext(
            connection.VenueId,
            connection.ExternalMerchantId,
            credentialProtector.Unprotect(connection.ProtectedAccessToken)), cancellationToken).ConfigureAwait(false);
        if (catalog.ContinuationToken is not null)
            throw new InvalidOperationException("Square returned an incomplete catalog during price synchronization.");
        var changed = new List<Guid>();
        foreach (var source in catalog.Items.OrderBy(value => value.ExternalId, StringComparer.Ordinal))
        {
            if (source.Price < 0 || source.Price > 999999.99m ||
                !string.Equals(source.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase)) continue;
            var item = await mappings.GetMappedItemAsync(connection.VenueId, PosProvider.Square, source.ExternalId, cancellationToken).ConfigureAwait(false);
            var price = decimal.Round(source.Price, 2, MidpointRounding.AwayFromZero);
            if (item is null || item.Price == price) continue;
            item.Price = price;
            item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
            if (!await menus.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false))
                throw new InvalidOperationException("The Square price update could not be persisted.");
            changed.Add(item.Id);
        }
        if (changed.Count > 0)
            await notifier.NotifyVenueContentUpdatedAsync(connection.VenueId, new { change = "pos-price", itemIds = changed }, cancellationToken).ConfigureAwait(false);
    }

    private static bool TryInventoryCounts(JsonElement root, out JsonElement counts)
    {
        counts = default;
        return root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("object", out var value) && value.ValueKind == JsonValueKind.Object &&
            value.TryGetProperty("inventory_counts", out counts) && counts.ValueKind == JsonValueKind.Array;
    }

    private static bool TryString(JsonElement value, string name, out string result)
    {
        result = string.Empty;
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String) return false;
        result = property.GetString()?.Trim() ?? string.Empty;
        return result.Length is > 0 and <= 300;
    }
}

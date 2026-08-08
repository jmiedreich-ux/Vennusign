using System.Text.Json;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class CloverRealtimeSyncHandler(
    IPosConnectionRepository connections,
    IPosCatalogMappingRepository mappings,
    IMenuRepository menus,
    IEnumerable<IPosProvider> providers,
    IPosCredentialProtector credentialProtector,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : IPosWebhookEventHandler
{
    public bool CanHandle(PosProvider provider, string eventType) =>
        provider == PosProvider.Clover && eventType is "inventory.item.create" or "inventory.item.update" or "inventory.item.delete";

    public async Task HandleAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        if (!CanHandle(webhookEvent.Provider, webhookEvent.EventType)) return;
        var connection = await connections.GetByExternalMerchantIdAsync(
            PosProvider.Clover, webhookEvent.ExternalMerchantId, cancellationToken).ConfigureAwait(false);
        if (connection is null || connection.Status != PosConnectionStatus.Connected) return;

        try
        {
            using var document = JsonDocument.Parse(webhookEvent.Payload, new JsonDocumentOptions { MaxDepth = 8 });
            var objectId = Text(document.RootElement, "objectId");
            if (!objectId.StartsWith("I:", StringComparison.Ordinal) || objectId.Length == 2) return;
            var externalId = objectId[2..];
            PosInventoryItem source;
            if (webhookEvent.EventType == "inventory.item.delete")
            {
                source = new PosInventoryItem(externalId, false, 0, null, null);
            }
            else
            {
                var provider = providers.Single(value => value.Provider == PosProvider.Clover);
                var result = await provider.GetInventoryAsync(new PosProviderContext(
                    connection.VenueId,
                    connection.ExternalMerchantId,
                    credentialProtector.Unprotect(connection.ProtectedAccessToken),
                    [externalId]), cancellationToken).ConfigureAwait(false);
                source = result.Items.SingleOrDefault(value => value.ExternalItemId == externalId)
                    ?? throw new InvalidOperationException("Clover did not return the requested inventory item.");
            }

            var item = await mappings.GetMappedItemAsync(
                connection.VenueId, PosProvider.Clover, externalId, cancellationToken).ConfigureAwait(false);
            if (item is not null)
                await ApplyAsync(connection.VenueId, item, source, cancellationToken).ConfigureAwait(false);
            var now = timeProvider.GetUtcNow().UtcDateTime;
            connection.LastSyncAttemptUtc = now;
            connection.LastSyncedUtc = now;
            connection.ConsecutiveSyncFailures = 0;
            connection.NextSyncAttemptUtc = null;
            connection.LastSyncErrorCode = null;
            await connections.SaveAsync(connection.VenueId, connection, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            connection.LastSyncAttemptUtc = now;
            connection.ConsecutiveSyncFailures = Math.Min(connection.ConsecutiveSyncFailures + 1, 1000);
            connection.LastSyncErrorCode = "clover-webhook-sync-failed";
            await connections.SaveAsync(connection.VenueId, connection, cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private async Task ApplyAsync(Guid venueId, MenuItem item, PosInventoryItem source, CancellationToken cancellationToken)
    {
        var availabilityChanged = item.IsAvailable != source.IsAvailable;
        var quantityChanged = item.QuantityAvailable != source.QuantityAvailable;
        var priceChanged = false;
        decimal? price = null;
        if (source.Price is >= 0 and <= 999999.99m && string.Equals(source.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase))
        {
            price = decimal.Round(source.Price.Value, 2, MidpointRounding.AwayFromZero);
            priceChanged = item.Price != price.Value;
        }
        if (!availabilityChanged && !quantityChanged && !priceChanged) return;
        item.IsAvailable = source.IsAvailable;
        item.QuantityAvailable = source.QuantityAvailable;
        if (priceChanged) item.Price = price!.Value;
        item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (!await menus.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("The Clover inventory update could not be persisted.");
        if (availabilityChanged)
            await notifier.NotifyVenueItemAvailabilityChangedAsync(venueId, item.Id.ToString(), item.IsAvailable, cancellationToken).ConfigureAwait(false);
        if (quantityChanged || priceChanged)
            await notifier.NotifyVenueContentUpdatedAsync(venueId, new
            {
                change = "clover-inventory",
                itemId = item.Id,
                quantityChanged,
                priceChanged
            }, cancellationToken).ConfigureAwait(false);
    }

    private static string Text(JsonElement value, string property) =>
        value.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()?.Trim() ?? string.Empty
            : string.Empty;
}

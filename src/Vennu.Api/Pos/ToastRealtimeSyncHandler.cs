using System.Text.Json;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class ToastRealtimeSyncHandler(
    IPosConnectionRepository connections,
    IPosCatalogImportService catalogImport,
    IScreenUpdateNotifier notifier,
    IToastInventorySyncService inventorySync) : IPosWebhookEventHandler
{
    private static readonly HashSet<string> StockEvents = ["in_stock", "out_of_stock", "low_quantity"];

    public bool CanHandle(PosProvider provider, string eventType) =>
        provider == PosProvider.Toast && (eventType == "menus_updated" || StockEvents.Contains(eventType));

    public async Task HandleAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        if (!CanHandle(webhookEvent.Provider, webhookEvent.EventType)) return;
        var connection = await connections.GetByExternalMerchantIdAsync(
            PosProvider.Toast, webhookEvent.ExternalMerchantId, cancellationToken).ConfigureAwait(false);
        if (connection is null || connection.Status != PosConnectionStatus.Connected) return;

        if (webhookEvent.EventType == "menus_updated")
        {
            var result = await catalogImport.ImportAsync(connection.VenueId, PosProvider.Toast, cancellationToken).ConfigureAwait(false);
            if (result.CategoriesCreated + result.CategoriesUpdated + result.ItemsCreated + result.ItemsUpdated > 0)
                await notifier.NotifyVenueContentUpdatedAsync(connection.VenueId, new { change = "toast-menu", result.CompletedUtc }, cancellationToken).ConfigureAwait(false);
            return;
        }

        using var document = JsonDocument.Parse(webhookEvent.Payload, new JsonDocumentOptions { MaxDepth = 32 });
        if (!document.RootElement.TryGetProperty("details", out var details) || details.ValueKind != JsonValueKind.Object) return;
        var externalId = Text(details, "itemGuid");
        if (externalId.Length == 0) return;
        var available = webhookEvent.EventType != "out_of_stock";
        int? quantity = null;
        if (details.TryGetProperty("quantity", out var rawQuantity) && rawQuantity.TryGetDecimal(out var numeric) &&
            numeric >= 0 && numeric <= int.MaxValue && decimal.Truncate(numeric) == numeric)
            quantity = (int)numeric;
        if (webhookEvent.EventType == "out_of_stock") quantity = 0;

        await inventorySync.ApplyItemAsync(
            connection.VenueId,
            new PosInventoryItem(externalId, available, quantity, null, null),
            cancellationToken).ConfigureAwait(false);
    }

    private static string Text(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()?.Trim() ?? string.Empty
            : string.Empty;
}

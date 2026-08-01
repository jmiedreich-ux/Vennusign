using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public interface IToastInventoryGateway
{
    Task<PosInventoryResult> GetInventoryAsync(
        string restaurantGuid,
        string accessToken,
        IReadOnlyCollection<string> itemGuids,
        CancellationToken cancellationToken = default);
}

public sealed class ToastInventoryGateway(
    HttpClient httpClient,
    IOptions<ToastInventoryOptions> options,
    TimeProvider timeProvider) : IToastInventoryGateway
{
    public async Task<PosInventoryResult> GetInventoryAsync(
        string restaurantGuid,
        string accessToken,
        IReadOnlyCollection<string> itemGuids,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(restaurantGuid, out _))
            throw new ArgumentException("A Toast restaurant GUID is required.", nameof(restaurantGuid));
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentNullException.ThrowIfNull(itemGuids);
        var endpoint = RequireEndpoint(options.Value.Endpoint);
        var batchSize = Math.Clamp(options.Value.MaximumItemsPerRequest, 1, 500);
        var normalized = itemGuids
            .Select(value => Guid.TryParse(value, out var parsed) ? parsed.ToString() : throw new ArgumentException("Toast inventory item identifiers must be GUIDs.", nameof(itemGuids)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var items = new Dictionary<string, PosInventoryItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var batch in normalized.Chunk(batchSize))
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(new { guids = batch })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            request.Headers.Add("Toast-Restaurant-External-ID", restaurantGuid.Trim());
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
            foreach (var item in Map(document.RootElement))
                items[item.ExternalItemId] = item;
        }

        if (items.Count != normalized.Length)
            throw new InvalidDataException("Toast inventory did not return a valid state for every requested item.");

        return new PosInventoryResult(
            items.Values.OrderBy(value => value.ExternalItemId, StringComparer.Ordinal).ToArray(),
            timeProvider.GetUtcNow());
    }

    internal static IReadOnlyCollection<PosInventoryItem> Map(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array) return [];
        var items = new List<PosInventoryItem>();
        foreach (var value in root.EnumerateArray())
        {
            if (Text(value, "itemGuidValidity") is { Length: > 0 } validity && validity != "VALID") continue;
            if (!Guid.TryParse(Text(value, "guid"), out var itemGuid)) continue;
            var status = Text(value, "status");
            if (status is not ("IN_STOCK" or "OUT_OF_STOCK" or "QUANTITY")) continue;
            int? quantity = null;
            if (status == "OUT_OF_STOCK") quantity = 0;
            else if (status == "QUANTITY" && value.TryGetProperty("quantity", out var rawQuantity) &&
                     rawQuantity.TryGetDecimal(out var numeric) && numeric >= 0 && numeric <= int.MaxValue && decimal.Truncate(numeric) == numeric)
                quantity = (int)numeric;
            items.Add(new PosInventoryItem(itemGuid.ToString(), status != "OUT_OF_STOCK", quantity, null, null));
        }
        return items;
    }

    private static string Text(JsonElement value, string name) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()?.Trim().ToUpperInvariant() ?? string.Empty
            : string.Empty;

    private static Uri RequireEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            uri.Host is not ("ws-api.toasttab.com" or "ws-sandbox-api.toasttab.com") ||
            !uri.AbsolutePath.Equals("/stock/v1/inventory/search", StringComparison.Ordinal))
            throw new InvalidOperationException("The Toast inventory endpoint must use the official HTTPS stock search resource.");
        return uri;
    }
}

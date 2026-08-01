using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class SquareCatalogGateway(HttpClient httpClient, IOptions<SquareCatalogOptions> options)
    : ISquareCatalogGateway
{
    public async Task<PosCatalogResult> GetCatalogAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        var value = options.Value;
        var endpoint = RequireEndpoint(value.Endpoint);
        var objects = new List<JsonElement>();
        string? cursor = null;
        do
        {
            var uri = new UriBuilder(endpoint) { Query = $"types=CATEGORY%2CITEM%2CMODIFIER_LIST{(cursor is null ? string.Empty : $"&cursor={Uri.EscapeDataString(cursor)}")}" }.Uri;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            request.Headers.Add("Square-Version", value.ApiVersion);
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
            if (document.RootElement.TryGetProperty("objects", out var pageObjects))
                objects.AddRange(pageObjects.EnumerateArray().Select(element => element.Clone()));
            cursor = document.RootElement.TryGetProperty("cursor", out var cursorElement) ? cursorElement.GetString() : null;
        } while (!string.IsNullOrWhiteSpace(cursor));

        return Map(objects);
    }

    internal static PosCatalogResult Map(IReadOnlyCollection<JsonElement> objects)
    {
        var categories = objects
            .Where(value => Type(value) == "CATEGORY")
            .Select((value, index) => new PosCatalogCategory(Id(value), Text(value, "category_data", "name"), index))
            .Where(value => value.ExternalId.Length > 0 && value.Name.Length > 0)
            .OrderBy(value => value.ExternalId, StringComparer.Ordinal)
            .Select((value, index) => value with { SortOrder = index })
            .ToArray();
        var modifierLists = objects
            .Where(value => Type(value) == "MODIFIER_LIST" && Id(value).Length > 0)
            .ToDictionary(Id, MapModifiers, StringComparer.Ordinal);
        var items = new List<PosCatalogItem>();
        foreach (var item in objects.Where(value => Type(value) == "ITEM").OrderBy(Id, StringComparer.Ordinal))
        {
            if (!item.TryGetProperty("item_data", out var data)) continue;
            var categoryId = CategoryId(data);
            var modifierIds = data.TryGetProperty("modifier_list_info", out var info)
                ? info.EnumerateArray().Select(value => Text(value, "modifier_list_id")).Where(value => value.Length > 0).ToArray()
                : [];
            var modifiers = modifierIds.SelectMany(id => modifierLists.GetValueOrDefault(id) ?? []).ToArray();
            if (!data.TryGetProperty("variations", out var variations)) continue;
            foreach (var variation in variations.EnumerateArray().OrderBy(Id, StringComparer.Ordinal))
            {
                if (!variation.TryGetProperty("item_variation_data", out var variationData)) continue;
                var money = variationData.TryGetProperty("price_money", out var priceMoney) ? priceMoney : default;
                var amount = money.ValueKind == JsonValueKind.Object && money.TryGetProperty("amount", out var amountElement)
                    ? amountElement.GetInt64() / 100m : 0m;
                var currency = money.ValueKind == JsonValueKind.Object ? Text(money, "currency") : string.Empty;
                var itemName = Text(data, "name");
                var variationName = Text(variationData, "name");
                items.Add(new PosCatalogItem(
                    Id(variation), categoryId,
                    variationName.Length > 0 && !string.Equals(variationName, "Regular", StringComparison.OrdinalIgnoreCase)
                        ? $"{itemName} — {variationName}" : itemName,
                    TextOrNull(data, "description"), amount, currency, modifiers));
            }
        }
        return new PosCatalogResult(categories, items.OrderBy(value => value.ExternalId, StringComparer.Ordinal).ToArray());
    }

    private static PosCatalogModifier[] MapModifiers(JsonElement value)
    {
        if (!value.TryGetProperty("modifier_list_data", out var data) || !data.TryGetProperty("modifiers", out var modifiers)) return [];
        return modifiers.EnumerateArray().Select(modifier =>
        {
            var modifierData = modifier.TryGetProperty("modifier_data", out var nested) ? nested : default;
            var money = modifierData.ValueKind == JsonValueKind.Object && modifierData.TryGetProperty("price_money", out var price) ? price : default;
            var amount = money.ValueKind == JsonValueKind.Object && money.TryGetProperty("amount", out var raw) ? raw.GetInt64() / 100m : 0m;
            return new PosCatalogModifier(Id(modifier), Text(modifierData, "name"), amount);
        }).Where(modifier => modifier.ExternalId.Length > 0 && modifier.Name.Length > 0).ToArray();
    }

    private static string CategoryId(JsonElement data)
    {
        if (data.TryGetProperty("categories", out var categories))
            return categories.EnumerateArray().Select(value => Text(value, "id")).FirstOrDefault(value => value.Length > 0) ?? string.Empty;
        return Text(data, "category_id");
    }

    private static string Type(JsonElement value) => Text(value, "type");
    private static string Id(JsonElement value) => Text(value, "id");
    private static string Text(JsonElement value, string property) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(property, out var element) ? element.GetString()?.Trim() ?? string.Empty : string.Empty;
    private static string Text(JsonElement value, string parent, string property) =>
        value.TryGetProperty(parent, out var nested) ? Text(nested, property) : string.Empty;
    private static string? TextOrNull(JsonElement value, string property) => Text(value, property) is { Length: > 0 } text ? text : null;

    private static Uri RequireEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(uri.Host, "connect.squareup.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The Square catalog endpoint must use the official HTTPS host.");
        return uri;
    }
}

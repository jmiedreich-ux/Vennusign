using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public interface ICloverCatalogGateway
{
    Task<PosCatalogResult> GetCatalogAsync(
        string merchantId,
        string accessToken,
        CancellationToken cancellationToken = default);
}

public sealed class CloverCatalogGateway(HttpClient httpClient, IOptions<CloverCatalogOptions> options)
    : ICloverCatalogGateway
{
    private const string UncategorizedId = "clover-uncategorized";

    public async Task<PosCatalogResult> GetCatalogAsync(
        string merchantId,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(merchantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        if (merchantId.Trim().Length > 128) throw new ArgumentException("Merchant ID is too long.", nameof(merchantId));
        var value = RequireOptions();
        var categories = await GetAllAsync(value, merchantId, accessToken, "categories", null, cancellationToken).ConfigureAwait(false);
        var items = await GetAllAsync(value, merchantId, accessToken, "items", "categories%2CmodifierGroups", cancellationToken).ConfigureAwait(false);
        var modifiers = await GetAllAsync(value, merchantId, accessToken, "modifiers", null, cancellationToken).ConfigureAwait(false);
        return Map(categories, items, modifiers, value.CurrencyCode);
    }

    internal static PosCatalogResult Map(
        IReadOnlyCollection<JsonElement> categoryElements,
        IReadOnlyCollection<JsonElement> itemElements,
        IReadOnlyCollection<JsonElement> modifierElements,
        string currencyCode)
    {
        var categories = categoryElements
            .Where(value => !Boolean(value, "deleted"))
            .Select(value => new PosCatalogCategory(Id(value), Text(value, "name"), Integer(value, "sortOrder")))
            .Where(value => value.ExternalId.Length > 0 && value.Name.Length > 0)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.ExternalId, StringComparer.Ordinal)
            .ToList();
        var modifierLookup = modifierElements
            .Where(value => !Boolean(value, "deleted"))
            .Select(value => new
            {
                GroupId = NestedId(value, "modifierGroup"),
                Modifier = new PosCatalogModifier(Id(value), Text(value, "name"), Money(value, "price"))
            })
            .Where(value => value.GroupId.Length > 0 && value.Modifier.ExternalId.Length > 0 && value.Modifier.Name.Length > 0)
            .GroupBy(value => value.GroupId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(value => value.Modifier).OrderBy(value => value.ExternalId, StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);
        var items = new List<PosCatalogItem>();
        var requiresUncategorized = false;
        foreach (var item in itemElements.Where(value => !Boolean(value, "deleted") && !Boolean(value, "hidden")).OrderBy(Id, StringComparer.Ordinal))
        {
            var itemId = Id(item);
            var itemName = Text(item, "name");
            if (itemId.Length == 0 || itemName.Length == 0) continue;
            var categoryId = ExpandedElements(item, "categories")
                .Where(value => !Boolean(value, "deleted"))
                .Select(Id)
                .Where(value => value.Length > 0)
                .OrderBy(value => value, StringComparer.Ordinal)
                .FirstOrDefault();
            if (categoryId is null)
            {
                categoryId = UncategorizedId;
                requiresUncategorized = true;
            }
            var groupIds = ExpandedElements(item, "modifierGroups")
                .Select(Id)
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal);
            var modifiers = groupIds.SelectMany(id => modifierLookup.GetValueOrDefault(id) ?? []).ToArray();
            var fixedPrice = string.Equals(Text(item, "priceType"), "FIXED", StringComparison.OrdinalIgnoreCase);
            items.Add(new PosCatalogItem(
                itemId,
                categoryId,
                itemName,
                null,
                fixedPrice ? Money(item, "price") : 0m,
                fixedPrice ? currencyCode.Trim().ToUpperInvariant() : string.Empty,
                modifiers));
        }
        if (requiresUncategorized)
            categories.Add(new PosCatalogCategory(UncategorizedId, "Uncategorized", int.MaxValue));
        return new PosCatalogResult(categories, items);
    }

    private async Task<IReadOnlyCollection<JsonElement>> GetAllAsync(
        CloverCatalogOptions value,
        string merchantId,
        string accessToken,
        string resource,
        string? expand,
        CancellationToken cancellationToken)
    {
        var all = new List<JsonElement>();
        for (var offset = 0; ; offset += value.PageSize)
        {
            var path = $"v3/merchants/{Uri.EscapeDataString(merchantId.Trim())}/{resource}";
            var query = $"limit={value.PageSize}&offset={offset}" + (expand is null ? string.Empty : $"&expand={expand}");
            var uri = new UriBuilder(new Uri(RequireBaseUrl(value.BaseUrl), path)) { Query = query }.Uri;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
            var page = document.RootElement.TryGetProperty("elements", out var elements)
                ? elements.EnumerateArray().Select(element => element.Clone()).ToArray()
                : [];
            all.AddRange(page);
            if (page.Length < value.PageSize) return all;
        }
    }

    private CloverCatalogOptions RequireOptions()
    {
        var value = options.Value;
        RequireBaseUrl(value.BaseUrl);
        if (value.PageSize is < 1 or > 1000) throw new InvalidOperationException("Clover catalog page size must be between 1 and 1000.");
        if (string.IsNullOrWhiteSpace(value.CurrencyCode) || value.CurrencyCode.Trim().Length != 3)
            throw new InvalidOperationException("Clover catalog currency code must contain three characters.");
        return value;
    }

    private static Uri RequireBaseUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            uri.Host is not ("apisandbox.dev.clover.com" or "api.clover.com" or "api.eu.clover.com" or "api.la.clover.com") ||
            uri.AbsolutePath != "/")
            throw new InvalidOperationException("The Clover catalog base URL must use an official HTTPS host root.");
        return uri;
    }

    private static IEnumerable<JsonElement> ExpandedElements(JsonElement value, string property) =>
        value.TryGetProperty(property, out var expanded) && expanded.TryGetProperty("elements", out var elements)
            ? elements.EnumerateArray()
            : [];
    private static string Id(JsonElement value) => Text(value, "id");
    private static string NestedId(JsonElement value, string property) =>
        value.TryGetProperty(property, out var nested) ? Id(nested) : string.Empty;
    private static string Text(JsonElement value, string property) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()?.Trim() ?? string.Empty
            : string.Empty;
    private static bool Boolean(JsonElement value, string property) =>
        value.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.True;
    private static int Integer(JsonElement value, string property) =>
        value.TryGetProperty(property, out var element) && element.TryGetInt32(out var result) ? result : 0;
    private static decimal Money(JsonElement value, string property) =>
        value.TryGetProperty(property, out var element) && element.TryGetInt64(out var cents) ? cents / 100m : 0m;
}

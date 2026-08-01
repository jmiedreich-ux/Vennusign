using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public interface IToastCatalogGateway
{
    Task<PosCatalogResult> GetCatalogAsync(
        string restaurantGuid,
        string accessToken,
        CancellationToken cancellationToken = default);
}

public sealed class ToastCatalogGateway(HttpClient httpClient, IOptions<ToastCatalogOptions> options)
    : IToastCatalogGateway
{
    public async Task<PosCatalogResult> GetCatalogAsync(
        string restaurantGuid,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(restaurantGuid, out _))
            throw new ArgumentException("A Toast restaurant GUID is required.", nameof(restaurantGuid));
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        var value = options.Value;
        var endpoint = RequireEndpoint(value.Endpoint);
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
        request.Headers.Add("Toast-Restaurant-External-ID", restaurantGuid.Trim());
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        return Map(document.RootElement, value.CurrencyCode);
    }

    internal static PosCatalogResult Map(JsonElement root, string currencyCode = "USD")
    {
        var categories = new Dictionary<string, PosCatalogCategory>(StringComparer.Ordinal);
        var items = new Dictionary<string, PosCatalogItem>(StringComparer.Ordinal);
        if (!root.TryGetProperty("menus", out var menus) || menus.ValueKind != JsonValueKind.Array)
            return new PosCatalogResult([], []);

        var sort = 0;
        foreach (var menu in menus.EnumerateArray())
        {
            if (!menu.TryGetProperty("menuGroups", out var groups) || groups.ValueKind != JsonValueKind.Array) continue;
            foreach (var group in groups.EnumerateArray())
            {
                var categoryId = Text(group, "guid");
                var categoryName = Text(group, "name");
                if (categoryId.Length == 0 || categoryName.Length == 0) continue;
                categories.TryAdd(categoryId, new PosCatalogCategory(categoryId, categoryName, sort++));
                if (!group.TryGetProperty("menuItems", out var menuItems) || menuItems.ValueKind != JsonValueKind.Array) continue;
                foreach (var item in menuItems.EnumerateArray())
                {
                    var externalId = Text(item, "guid");
                    var name = Text(item, "name");
                    if (externalId.Length == 0 || name.Length == 0) continue;
                    var price = item.TryGetProperty("price", out var rawPrice) && rawPrice.TryGetDecimal(out var amount) ? amount : -1m;
                    items[externalId] = new PosCatalogItem(
                        externalId,
                        categoryId,
                        name,
                        Text(item, "description") is { Length: > 0 } description ? description : null,
                        price,
                        currencyCode.Trim().ToUpperInvariant(),
                        []);
                }
            }
        }

        return new PosCatalogResult(
            categories.Values.OrderBy(value => value.SortOrder).ToArray(),
            items.Values.OrderBy(value => value.ExternalId, StringComparer.Ordinal).ToArray());
    }

    private static string Text(JsonElement value, string property) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()?.Trim() ?? string.Empty
            : string.Empty;

    private static Uri RequireEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            uri.Host is not ("ws-api.toasttab.com" or "ws-sandbox-api.toasttab.com"))
            throw new InvalidOperationException("The Toast catalog endpoint must use an official HTTPS host.");
        return uri;
    }
}

public sealed class ToastPosProvider(IToastCatalogGateway catalogGateway, IToastInventoryGateway inventoryGateway) : IPosProvider
{
    public PosProvider Provider => PosProvider.Toast;

    public Task<PosCatalogResult> GetCatalogAsync(PosProviderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return catalogGateway.GetCatalogAsync(context.ExternalMerchantId, context.AccessToken, cancellationToken);
    }

    public Task<PosInventoryResult> GetInventoryAsync(PosProviderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return inventoryGateway.GetInventoryAsync(
            context.ExternalMerchantId,
            context.AccessToken,
            context.InventoryExternalItemIds ?? [],
            cancellationToken);
    }
}

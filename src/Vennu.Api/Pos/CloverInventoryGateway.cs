using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public interface ICloverInventoryGateway
{
    Task<PosInventoryResult> GetInventoryAsync(
        string merchantId,
        string accessToken,
        IReadOnlyCollection<string> externalItemIds,
        CancellationToken cancellationToken = default);
}

public sealed class CloverInventoryGateway(
    HttpClient httpClient,
    IOptions<CloverCatalogOptions> options,
    TimeProvider timeProvider) : ICloverInventoryGateway
{
    private const int MaximumItems = 100;

    public async Task<PosInventoryResult> GetInventoryAsync(
        string merchantId,
        string accessToken,
        IReadOnlyCollection<string> externalItemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(merchantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentNullException.ThrowIfNull(externalItemIds);
        var ids = externalItemIds.Select(RequireId).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
        if (ids.Length > MaximumItems) throw new ArgumentException($"Clover inventory requests cannot exceed {MaximumItems} items.", nameof(externalItemIds));
        var value = options.Value;
        var baseUrl = RequireBaseUrl(value.BaseUrl);
        var currency = RequireCurrency(value.CurrencyCode);
        var items = new List<PosInventoryItem>(ids.Length);
        foreach (var externalId in ids)
        {
            var path = $"v3/merchants/{Uri.EscapeDataString(RequireId(merchantId))}/items/{Uri.EscapeDataString(externalId)}";
            var uri = new UriBuilder(new Uri(baseUrl, path)) { Query = "expand=itemStock" }.Uri;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
            items.Add(Map(document.RootElement, externalId, currency));
        }
        return new PosInventoryResult(items, timeProvider.GetUtcNow());
    }

    internal static PosInventoryItem Map(JsonElement value, string expectedId, string currencyCode)
    {
        var id = Text(value, "id");
        if (!string.Equals(id, expectedId, StringComparison.Ordinal))
            throw new InvalidOperationException("Clover returned inventory for a different item.");
        var deleted = Boolean(value, "deleted");
        var providerAvailable = !value.TryGetProperty("available", out var availableElement) || availableElement.ValueKind == JsonValueKind.True;
        int? quantity = null;
        if (value.TryGetProperty("itemStock", out var stock) && stock.ValueKind == JsonValueKind.Object &&
            stock.TryGetProperty("quantity", out var quantityElement) && quantityElement.TryGetDecimal(out var numeric) &&
            numeric >= 0 && numeric <= int.MaxValue && decimal.Truncate(numeric) == numeric)
            quantity = (int)numeric;
        long cents = 0;
        var fixedPrice = string.Equals(Text(value, "priceType"), "FIXED", StringComparison.OrdinalIgnoreCase) &&
            value.TryGetProperty("price", out var priceElement) && priceElement.TryGetInt64(out cents) && cents >= 0;
        return new PosInventoryItem(
            id,
            !deleted && providerAvailable && (quantity is null || quantity > 0),
            deleted ? 0 : quantity,
            fixedPrice ? cents / 100m : null,
            fixedPrice ? currencyCode : null);
    }

    private static Uri RequireBaseUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            uri.Host is not ("apisandbox.dev.clover.com" or "api.clover.com" or "api.eu.clover.com" or "api.la.clover.com") ||
            uri.AbsolutePath != "/")
            throw new InvalidOperationException("The Clover inventory base URL must use an official HTTPS host root.");
        return uri;
    }

    private static string RequireCurrency(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length == 3
            ? value.Trim().ToUpperInvariant()
            : throw new InvalidOperationException("Clover inventory currency code must contain three characters.");

    private static string RequireId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        return normalized.Length <= 200 ? normalized : throw new ArgumentException("Clover identifiers cannot exceed 200 characters.");
    }

    private static string Text(JsonElement value, string property) =>
        value.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString()?.Trim() ?? string.Empty
            : string.Empty;
    private static bool Boolean(JsonElement value, string property) =>
        value.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.True;
}

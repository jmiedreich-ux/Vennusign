using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IPosProvider
{
    PosProvider Provider { get; }

    Task<PosCatalogResult> GetCatalogAsync(
        PosProviderContext context,
        CancellationToken cancellationToken = default);

    Task<PosInventoryResult> GetInventoryAsync(
        PosProviderContext context,
        CancellationToken cancellationToken = default);
}

public sealed record PosProviderContext(
    Guid VenueId,
    string ExternalMerchantId,
    string AccessToken);

public sealed record PosCatalogResult(
    IReadOnlyCollection<PosCatalogCategory> Categories,
    IReadOnlyCollection<PosCatalogItem> Items,
    string? ContinuationToken = null);

public sealed record PosCatalogCategory(
    string ExternalId,
    string Name,
    int SortOrder);

public sealed record PosCatalogItem(
    string ExternalId,
    string ExternalCategoryId,
    string Name,
    string? Description,
    decimal Price,
    string CurrencyCode,
    IReadOnlyCollection<PosCatalogModifier> Modifiers);

public sealed record PosCatalogModifier(
    string ExternalId,
    string Name,
    decimal PriceAdjustment);

public sealed record PosInventoryResult(
    IReadOnlyCollection<PosInventoryItem> Items,
    DateTimeOffset ObservedUtc,
    string? ContinuationToken = null);

public sealed record PosInventoryItem(
    string ExternalItemId,
    bool IsAvailable,
    int? QuantityAvailable,
    decimal? Price,
    string? CurrencyCode);

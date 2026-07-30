using Vennu.Core.Models;

namespace Vennu.Api.Contracts.Admin;

public sealed record TapCategoryWriteRequest(string Name, decimal? CategoryPrice, bool IsActive);

public sealed record TapItemWriteRequest(
    Guid? TapCategoryId,
    string Name,
    string? Style,
    decimal? Abv,
    int? Ibu,
    string? Description,
    decimal Price,
    string? GlassColor,
    string? NameColor,
    bool IsAvailable,
    bool IsComingSoon);

public sealed record TapListAdministrationResponse(
    IReadOnlyCollection<TapCategory> Categories,
    IReadOnlyCollection<TapItem> Items);

public sealed record TapOrderRequest(IReadOnlyCollection<Guid> Ids);

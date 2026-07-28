namespace Vennu.Data.Services;

public sealed record VenueDirectoryQuery(
    string? Search = null,
    string? Tier = null,
    string? Status = null,
    string? Health = null);


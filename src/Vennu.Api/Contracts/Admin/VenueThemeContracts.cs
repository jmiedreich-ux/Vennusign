namespace Vennu.Api.Contracts.Admin;

public sealed record VenueThemeResponse(
    Guid VenueId,
    string BackgroundColor,
    string AccentColor,
    string FontFamily,
    DateTime UpdatedUtc);

public sealed record VenueThemeUpdateRequest(
    string BackgroundColor,
    string AccentColor,
    string FontFamily);

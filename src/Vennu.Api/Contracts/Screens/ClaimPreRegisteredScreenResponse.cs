namespace Vennu.Api.Contracts.Screens;

public sealed record ClaimPreRegisteredScreenResponse(
    Guid ScreenId,
    string ScreenKey,
    Guid VenueId,
    string DisplayPath);

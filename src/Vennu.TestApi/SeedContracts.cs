namespace Vennu.TestApi;

public sealed record SeedRequest(string? AccessToken, bool IncludeScreen = true, string? Label = null);
public sealed record BackdateAvailabilityRequest(string? AccessToken, Guid ItemId, int MinutesAgo);
public sealed record ScaleSeedRequest(string? AccessToken, int Menus = 13, int Screens = 20);
public sealed record ScaleSeedMenu(Guid MenuId, string Name, string State, IReadOnlyCollection<Guid> ScreenIds);
public sealed record ScaleSeedResponse(Guid VenueId, IReadOnlyCollection<ScaleSeedMenu> SeededMenus, IReadOnlyCollection<Guid> ScreenIds);

public sealed record SeedResponse(
    Guid OrganizationId,
    Guid VenueId,
    Guid MenuId,
    Guid SectionId,
    Guid ItemId,
    string MenuName,
    string SectionName,
    string ItemName,
    string ItemDescription,
    decimal ItemPrice,
    Guid? ScreenId,
    string? ScreenKey);

internal sealed record SessionResponse(Guid VenueId, Guid? OrganizationId);
internal sealed record MenuResponse(Guid Id, string Name);
internal sealed record SectionResponse(Guid SectionId, string Name, int SortOrder);
internal sealed record PlaceResponse(string Outcome, Guid? ItemId, Guid? SectionId, int SortOrder, int ItemCountOnMenu);
internal sealed record ScreenResponse(Guid Id, string Name, string RegistrationUrl);

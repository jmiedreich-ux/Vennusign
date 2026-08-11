namespace Vennu.TestApi;

public sealed record SeedRequest(
    string? AccessToken,
    bool IncludeScreen = true,
    string? Label = null,
    int SectionCount = 1,
    int ItemsPerSection = 1,
    IReadOnlyCollection<string>? LibraryItemNames = null,
    string ScreenState = ScreenSeedStates.Offline,
    int PageCount = 1,
    int ScreenWidthPixels = 1920,
    int ScreenHeightPixels = 1080);

public static class ScreenSeedStates
{
    public const string Offline = "offline";
    public const string Online = "online";
    public const string NeverPaired = "never-paired";
    public const string HasNotTakenThisYet = "has-not-taken-this-yet";
    public static bool IsSupported(string value) => value is Offline or Online or NeverPaired or HasNotTakenThisYet;
}
public sealed record BackdateAvailabilityRequest(string? AccessToken, Guid ItemId, int MinutesAgo);
public sealed record WriteHistoryAtRequest(string? AccessToken, Guid MenuId, string Kind, string? Detail, DateTime OccurredUtc);
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
    string? ScreenKey,
    IReadOnlyCollection<SeedSection>? Sections = null,
    IReadOnlyCollection<SeedItem>? Items = null,
    string? ScreenState = null,
    IReadOnlyCollection<SeedPage>? Pages = null);

public sealed record SeedPage(Guid PageId, string Name, int SortOrder);
public sealed record SeedSection(Guid SectionId, Guid PageId, string Name, int SortOrder);
public sealed record SeedItem(Guid ItemId, Guid SectionId, string Name, string Price);

internal sealed record SessionResponse(Guid VenueId, Guid? OrganizationId);
internal sealed record MenuResponse(Guid Id, string Name);
internal sealed record SectionResponse(Guid SectionId, string Name, int SortOrder);
internal sealed record PageResponse(Guid PageId, string Name, int SortOrder);
internal sealed record PlaceResponse(string Outcome, Guid? ItemId, Guid? SectionId, int SortOrder, int ItemCountOnMenu);
internal sealed record RegisteredScreenResponse(Guid ScreenId, string ScreenKey);
internal sealed record PairingCodeResponse(string Code, Guid ScreenId, DateTime ExpiresAt);
internal sealed record ClaimedScreenResponse(bool Linked, Guid ScreenId, Guid VenueId);
internal sealed record SeededScreen(Guid ScreenId, string ScreenKey);
internal sealed record HeartbeatResponse(Guid ScreenId, string Status, DateTime LastSeenUtc, string? Platform, string? AppVersion);

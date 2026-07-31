namespace Vennu.Api.Contracts.Admin;

public sealed record ScreenCreateRequest(string Name, string? Location);

public sealed record ScreenUpdateRequest(
    string Name,
    string? Location,
    string? PhotoGridDensity,
    string? DisplayLayout,
    string? SplitRatio,
    int? HeroDwellSeconds);

public sealed record ScreenManagementItem(
    Guid Id,
    string Name,
    string? Location,
    string PhotoGridDensity,
    string DisplayLayout,
    string SplitRatio,
    int HeroDwellSeconds,
    string Status,
    DateTime? LastSeen,
    string RegistrationUrl);

public sealed record ScreenPushAllResult(int ScreenCount);

public sealed record HaasPreRegistrationRequest(
    string Name,
    string? Location,
    string Platform,
    string DesiredAppVersion,
    string DeliveryReference,
    int? ExpiresInHours);

public sealed record HaasPreRegistrationResponse(
    Guid ScreenId,
    string ScreenKey,
    string Platform,
    string DesiredAppVersion,
    string DeliveryReference,
    DateTime ExpiresUtc,
    string BootstrapToken,
    string LaunchPath);

public sealed record ScreenOverflowItem(Guid ItemId, string SectionName, string ItemName, bool Visible);

public sealed record ScreenOverflowPreview(
    int Capacity,
    int TotalItems,
    int VisibleItems,
    int OverflowItems,
    IReadOnlyCollection<ScreenOverflowItem> Items);

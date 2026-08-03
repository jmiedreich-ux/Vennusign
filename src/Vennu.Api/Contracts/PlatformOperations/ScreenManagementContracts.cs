namespace Vennu.Api.Contracts.PlatformOperations;

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
    string? Platform,
    string? AppVersion,
    string RegistrationUrl,
    long? AuthoritativeRevision = null,
    long? AppliedRevision = null,
    string? DeliveryState = null,
    DateTime? DeliveryRequestedUtc = null,
    DateTime? DeliveryAppliedUtc = null,
    string? DeliveryFailureCode = null,
    string? DeliveryFailureDetail = null);

public sealed record ScreenPushResult(long Revision, string State, DateTime RequestedUtc);

public sealed record ScreenLifecycleRequest(bool Archived);

public sealed record ScreenReplacementRequest(Guid TargetScreenId, string PairingCode, bool Confirmed, DateTime? ExpectedTargetUpdatedUtc = null);

public sealed record ScreenReplacementResponse(
    string Status,
    Guid? TargetScreenId,
    Guid? SourceScreenId,
    string? TargetName,
    string? ReplacementPlatform,
    string? ReplacementAppVersion,
    string? WallGroup,
    int? WallPosition,
    bool PreservesConfiguration,
    bool PreservesHistory,
    bool PreservesVideoWall,
    DateTime? TargetUpdatedUtc,
    DateTime? CompletedUtc);

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

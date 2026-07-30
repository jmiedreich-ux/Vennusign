namespace Vennu.Api.Contracts.Admin;

public sealed record ScreenCreateRequest(string Name, string? Location);

public sealed record ScreenUpdateRequest(string Name, string? Location);

public sealed record ScreenManagementItem(
    Guid Id,
    string Name,
    string? Location,
    string Status,
    DateTime? LastSeen,
    string RegistrationUrl);

public sealed record ScreenPushAllResult(int ScreenCount);

public sealed record ScreenOverflowItem(Guid ItemId, string SectionName, string ItemName, bool Visible);

public sealed record ScreenOverflowPreview(
    int Capacity,
    int TotalItems,
    int VisibleItems,
    int OverflowItems,
    IReadOnlyCollection<ScreenOverflowItem> Items);

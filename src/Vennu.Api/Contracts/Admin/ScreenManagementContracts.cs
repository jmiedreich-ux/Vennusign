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

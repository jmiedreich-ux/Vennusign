namespace Vennu.Api.Contracts.BackOffice;

public sealed record BackOfficeSessionResponse(
    Guid VenueId,
    string DisplayName,
    IReadOnlyCollection<string> Capabilities,
    Guid? OrganizationId,
    string OrganizationName,
    string VenueName,
    BackOfficeAccountResponse Account,
    IReadOnlyCollection<BackOfficeContextResponse> Contexts);

public sealed record BackOfficeAccountResponse(Guid? UserId, string DisplayName, string? Email);

public sealed record BackOfficeContextResponse(
    Guid OrganizationId,
    string OrganizationName,
    Guid VenueId,
    string VenueName);

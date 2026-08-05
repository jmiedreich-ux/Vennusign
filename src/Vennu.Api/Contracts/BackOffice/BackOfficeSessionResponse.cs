namespace Vennu.Api.Contracts.BackOffice;

public sealed record BackOfficeSessionResponse(
    Guid VenueId,
    string DisplayName,
    IReadOnlyCollection<BackOfficeCapabilityDecisionResponse> CapabilityDecisions,
    Guid? OrganizationId,
    string OrganizationName,
    string VenueName,
    BackOfficeAccountResponse Account,
    IReadOnlyCollection<BackOfficeContextResponse> Contexts);

public sealed record BackOfficeCapabilityDecisionResponse(
    string CapabilityId,
    string Decision,
    string ReasonCode,
    string Category,
    string Message,
    string? Resolution,
    int? RetryAfterSeconds)
{
    public bool IsAllowed => Decision is "allowed" or "allowed-with-conditions";
}

public sealed record BackOfficeAccountResponse(Guid? UserId, string DisplayName, string? Email);

public sealed record BackOfficeContextResponse(
    Guid OrganizationId,
    string OrganizationName,
    Guid VenueId,
    string VenueName);

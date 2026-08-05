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
    string MessageKey,
    string Message,
    IReadOnlyDictionary<string, string> Parameters,
    string CorrelationId,
    string Locale,
    string? Resolution,
    int? RetryAfterSeconds,
    IReadOnlyCollection<BackOfficeCapabilityDecisionConditionResponse> Conditions)
{
    public bool IsAllowed => Decision is "allowed" or "allowed-with-conditions";
}

public sealed record BackOfficeCapabilityDecisionConditionResponse(
    string Category,
    string ReasonCode,
    string MessageKey,
    string Message,
    IReadOnlyDictionary<string, string> Parameters,
    string? Resolution);

public sealed record BackOfficeAccountResponse(Guid? UserId, string DisplayName, string? Email);

public sealed record BackOfficeContextResponse(
    Guid OrganizationId,
    string OrganizationName,
    Guid VenueId,
    string VenueName);

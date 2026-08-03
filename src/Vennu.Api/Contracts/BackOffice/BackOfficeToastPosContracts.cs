namespace Vennu.Api.Contracts.BackOffice;

public sealed record ConfigureToastConnectionRequest(string RestaurantGuid, string AccessToken);

public sealed record BackOfficeToastStatusResponse(
    BackOfficePosConnectionResponse? Connection,
    string WebhookRegistrationStatus,
    bool RequiresToastApproval,
    string Guidance,
    BackOfficeToastPollingHealthResponse? Polling = null);

public sealed record BackOfficeToastPollingHealthResponse(
    string State,
    DateTime? LastAttemptUtc,
    DateTime? LastSucceededUtc,
    int ConsecutiveFailures,
    DateTime? NextAttemptUtc,
    string? ErrorCode);

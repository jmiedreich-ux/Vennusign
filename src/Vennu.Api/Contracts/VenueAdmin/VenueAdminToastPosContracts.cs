namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record ConfigureToastConnectionRequest(string RestaurantGuid, string AccessToken);

public sealed record VenueAdminToastStatusResponse(
    VenueAdminPosConnectionResponse? Connection,
    string WebhookRegistrationStatus,
    bool RequiresToastApproval,
    string Guidance,
    VenueAdminToastPollingHealthResponse? Polling = null);

public sealed record VenueAdminToastPollingHealthResponse(
    string State,
    DateTime? LastAttemptUtc,
    DateTime? LastSucceededUtc,
    int ConsecutiveFailures,
    DateTime? NextAttemptUtc,
    string? ErrorCode);

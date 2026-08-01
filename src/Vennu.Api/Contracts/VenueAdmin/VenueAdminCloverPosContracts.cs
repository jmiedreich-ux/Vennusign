namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record VenueAdminCloverStatusResponse(
    VenueAdminPosConnectionResponse? Connection,
    string WebhookRegistrationStatus,
    bool RequiresExternalRegistration,
    string Guidance,
    DateTime? LastSyncedUtc,
    DateTime? LastSyncAttemptUtc,
    int ConsecutiveSyncFailures,
    string? LastSyncErrorCode);

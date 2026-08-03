namespace Vennu.Api.Contracts.BackOffice;

public sealed record BackOfficeCloverStatusResponse(
    BackOfficePosConnectionResponse? Connection,
    string WebhookRegistrationStatus,
    bool RequiresExternalRegistration,
    string Guidance,
    DateTime? LastSyncedUtc,
    DateTime? LastSyncAttemptUtc,
    int ConsecutiveSyncFailures,
    string? LastSyncErrorCode);

using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class PosWebhookEventRepository(ISqlDataAccess dataAccess, TimeProvider timeProvider)
    : IPosWebhookEventRepository
{
    private const string EnqueueSql = """
        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.PosWebhookEvents WITH (UPDLOCK, HOLDLOCK)
            WHERE Provider = @Provider AND ProviderEventId = @ProviderEventId
        )
        BEGIN
            INSERT dbo.PosWebhookEvents
                (Id, Provider, ProviderEventId, EventType, ExternalMerchantId, Payload,
                 Status, AttemptCount, ReceivedUtc)
            VALUES
                (@Id, @Provider, @ProviderEventId, @EventType, @ExternalMerchantId, @Payload,
                 0, 0, @ReceivedUtc);
            SELECT CAST(1 AS BIT) AS Changed;
        END
        ELSE SELECT CAST(0 AS BIT) AS Changed;
        """;

    private const string ClaimSql = """
        SET XACT_ABORT ON;
        -- READPAST is only legal under READ COMMITTED or REPEATABLE READ. Without this
        -- the claim throws whenever the ambient isolation level is anything else.
        SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
        BEGIN TRANSACTION;

        DECLARE @Id UNIQUEIDENTIFIER;
        SELECT TOP (1) @Id = Id
        FROM dbo.PosWebhookEvents WITH (UPDLOCK, READPAST, ROWLOCK)
        WHERE (Status IN (0, 3) AND (NextAttemptUtc IS NULL OR NextAttemptUtc <= @UtcNow))
           OR (Status = 1 AND StartedUtc <= @StaleBeforeUtc)
        ORDER BY ReceivedUtc, Id;

        UPDATE dbo.PosWebhookEvents
        SET Status = 1,
            AttemptCount = AttemptCount + 1,
            StartedUtc = @UtcNow,
            NextAttemptUtc = NULL,
            FailureReason = NULL
        OUTPUT inserted.Id, inserted.Provider, inserted.ProviderEventId, inserted.EventType,
               inserted.ExternalMerchantId, inserted.Payload, inserted.Status, inserted.AttemptCount,
               inserted.ReceivedUtc, inserted.StartedUtc, inserted.ProcessedUtc,
               inserted.NextAttemptUtc, inserted.FailureReason
        WHERE Id = @Id;

        COMMIT TRANSACTION;
        """;

    private const string MarkProcessedSql = """
        UPDATE dbo.PosWebhookEvents
        SET Status = 2, ProcessedUtc = @ProcessedUtc, FailureReason = NULL, NextAttemptUtc = NULL
        OUTPUT CAST(1 AS BIT) AS Changed
        WHERE Id = @Id AND Status = 1;
        """;

    private const string MarkFailedSql = """
        UPDATE dbo.PosWebhookEvents
        SET Status = 3, ProcessedUtc = @FailedUtc, FailureReason = @FailureReason,
            NextAttemptUtc = @NextAttemptUtc
        OUTPUT CAST(1 AS BIT) AS Changed
        WHERE Id = @Id AND Status = 1;
        """;

    public async Task<bool> EnqueueAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        webhookEvent.Id = webhookEvent.Id == Guid.Empty ? Guid.NewGuid() : webhookEvent.Id;
        webhookEvent.ProviderEventId = Normalize(webhookEvent.ProviderEventId, nameof(webhookEvent.ProviderEventId), 300);
        webhookEvent.EventType = Normalize(webhookEvent.EventType, nameof(webhookEvent.EventType), 200);
        webhookEvent.ExternalMerchantId = Normalize(webhookEvent.ExternalMerchantId, nameof(webhookEvent.ExternalMerchantId), 200);
        if (!Enum.IsDefined(webhookEvent.Provider)) throw new ArgumentOutOfRangeException(nameof(webhookEvent));
        if (string.IsNullOrWhiteSpace(webhookEvent.Payload)) throw new ArgumentException("A payload is required.", nameof(webhookEvent));
        webhookEvent.ReceivedUtc = webhookEvent.ReceivedUtc == default ? timeProvider.GetUtcNow().UtcDateTime : webhookEvent.ReceivedUtc;
        return (await dataAccess.ExecuteSqlQueryAsync<ChangeResult, object>(EnqueueSql, new
        {
            webhookEvent.Id,
            Provider = (int)webhookEvent.Provider,
            webhookEvent.ProviderEventId,
            webhookEvent.EventType,
            webhookEvent.ExternalMerchantId,
            webhookEvent.Payload,
            webhookEvent.ReceivedUtc
        }, cancellationToken).ConfigureAwait(false)).Single().Changed;
    }

    public async Task<PosWebhookEvent?> TryClaimNextAsync(DateTime utcNow, DateTime staleBeforeUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PosWebhookEvent, object>(ClaimSql, new { UtcNow = utcNow, StaleBeforeUtc = staleBeforeUtc }, cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<bool> MarkProcessedAsync(Guid id, DateTime processedUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ChangeResult, object>(MarkProcessedSql, new { Id = RequireId(id), ProcessedUtc = processedUtc }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Changed ?? false;

    public async Task<bool> MarkFailedAsync(Guid id, string failureReason, DateTime failedUtc, DateTime nextAttemptUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ChangeResult, object>(MarkFailedSql, new
        {
            Id = RequireId(id),
            FailureReason = Normalize(failureReason, nameof(failureReason), 500),
            FailedUtc = failedUtc,
            NextAttemptUtc = nextAttemptUtc
        }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Changed ?? false;

    private static Guid RequireId(Guid id) => id != Guid.Empty ? id : throw new ArgumentException("A non-empty identifier is required.", nameof(id));

    private static string Normalize(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var result = value.Trim();
        return result.Length <= maximumLength ? result : throw new ArgumentException($"The value cannot exceed {maximumLength} characters.", parameterName);
    }

    public sealed class ChangeResult { public bool Changed { get; set; } }
}

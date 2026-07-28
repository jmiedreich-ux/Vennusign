using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class ProcessedStripeEventRepository : IProcessedStripeEventRepository
{
    private const string ClaimSql = """
        MERGE dbo.ProcessedStripeEvents WITH (HOLDLOCK) AS target
        USING (SELECT @EventId AS EventId) AS source
           ON target.EventId = source.EventId
        WHEN MATCHED AND (
             target.Status = 'failed'
             OR (target.Status = 'processing' AND target.StartedUtc <= @StaleBeforeUtc))
          THEN UPDATE
             SET EventType = @EventType,
                 Status = 'processing',
                 StartedUtc = @UtcNow,
                 ProcessedUtc = NULL,
                 FailureReason = NULL
        WHEN NOT MATCHED
          THEN INSERT (EventId, EventType, Status, StartedUtc)
               VALUES (@EventId, @EventType, 'processing', @UtcNow)
        OUTPUT inserted.EventId,
               inserted.EventType,
               inserted.Status,
               inserted.StartedUtc,
               inserted.ProcessedUtc,
               inserted.FailureReason;
        """;

    private const string MarkProcessedSql = """
        UPDATE dbo.ProcessedStripeEvents
        SET Status = 'processed',
            ProcessedUtc = @ProcessedUtc,
            FailureReason = NULL
        OUTPUT inserted.EventId,
               inserted.EventType,
               inserted.Status,
               inserted.StartedUtc,
               inserted.ProcessedUtc,
               inserted.FailureReason
        WHERE EventId = @EventId
          AND Status = 'processing';
        """;

    private const string MarkFailedSql = """
        UPDATE dbo.ProcessedStripeEvents
        SET Status = 'failed',
            ProcessedUtc = @FailedUtc,
            FailureReason = @FailureReason
        OUTPUT inserted.EventId,
               inserted.EventType,
               inserted.Status,
               inserted.StartedUtc,
               inserted.ProcessedUtc,
               inserted.FailureReason
        WHERE EventId = @EventId
          AND Status = 'processing';
        """;

    private readonly ISqlDataAccess dataAccess;

    public ProcessedStripeEventRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<ProcessedStripeEvent?> TryClaimAsync(
        string eventId,
        string eventType,
        DateTime utcNow,
        DateTime staleBeforeUtc,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ProcessedStripeEvent, object>(
            ClaimSql,
            new { EventId = eventId, EventType = eventType, UtcNow = utcNow, StaleBeforeUtc = staleBeforeUtc },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<bool> MarkProcessedAsync(
        string eventId,
        DateTime processedUtc,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ProcessedStripeEvent, object>(
            MarkProcessedSql,
            new { EventId = eventId, ProcessedUtc = processedUtc },
            cancellationToken).ConfigureAwait(false)).Any();

    public async Task<bool> MarkFailedAsync(
        string eventId,
        string failureReason,
        DateTime failedUtc,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ProcessedStripeEvent, object>(
            MarkFailedSql,
            new { EventId = eventId, FailureReason = failureReason, FailedUtc = failedUtc },
            cancellationToken).ConfigureAwait(false)).Any();
}

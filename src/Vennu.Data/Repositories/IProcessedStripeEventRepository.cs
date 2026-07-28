using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IProcessedStripeEventRepository
{
    Task<ProcessedStripeEvent?> TryClaimAsync(
        string eventId,
        string eventType,
        DateTime utcNow,
        DateTime staleBeforeUtc,
        CancellationToken cancellationToken = default);

    Task<bool> MarkProcessedAsync(
        string eventId,
        DateTime processedUtc,
        CancellationToken cancellationToken = default);

    Task<bool> MarkFailedAsync(
        string eventId,
        string failureReason,
        DateTime failedUtc,
        CancellationToken cancellationToken = default);
}

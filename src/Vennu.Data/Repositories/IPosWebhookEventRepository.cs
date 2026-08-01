using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IPosWebhookEventRepository
{
    Task<bool> EnqueueAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task<PosWebhookEvent?> TryClaimNextAsync(DateTime utcNow, DateTime staleBeforeUtc, CancellationToken cancellationToken = default);
    Task<bool> MarkProcessedAsync(Guid id, DateTime processedUtc, CancellationToken cancellationToken = default);
    Task<bool> MarkFailedAsync(Guid id, string failureReason, DateTime failedUtc, DateTime nextAttemptUtc, CancellationToken cancellationToken = default);
}

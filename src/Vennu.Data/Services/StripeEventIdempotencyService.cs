using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class StripeEventIdempotencyService : IStripeEventIdempotencyService
{
    private static readonly TimeSpan ProcessingLease = TimeSpan.FromMinutes(5);
    private const int MaximumFailureReasonLength = 500;
    private readonly IProcessedStripeEventRepository repository;
    private readonly TimeProvider timeProvider;

    public StripeEventIdempotencyService(
        IProcessedStripeEventRepository repository,
        TimeProvider timeProvider)
    {
        this.repository = repository;
        this.timeProvider = timeProvider;
    }

    public async Task<bool> ExecuteOnceAsync(
        string eventId,
        string eventType,
        Func<CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        eventId = Normalize(eventId, nameof(eventId));
        eventType = Normalize(eventType, nameof(eventType));
        ArgumentNullException.ThrowIfNull(handler);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var claim = await repository.TryClaimAsync(
            eventId,
            eventType,
            utcNow,
            utcNow.Subtract(ProcessingLease),
            cancellationToken).ConfigureAwait(false);
        if (claim is null)
        {
            return false;
        }

        try
        {
            await handler(cancellationToken).ConfigureAwait(false);
            if (!await repository.MarkProcessedAsync(
                eventId,
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Stripe event '{eventId}' lost its processing claim.");
            }

            return true;
        }
        catch (Exception exception)
        {
            var failureReason = exception.Message.Length <= MaximumFailureReasonLength
                ? exception.Message
                : exception.Message[..MaximumFailureReasonLength];
            await repository.MarkFailedAsync(
                eventId,
                failureReason,
                timeProvider.GetUtcNow().UtcDateTime,
                CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    private static string Normalize(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}

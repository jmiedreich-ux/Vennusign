namespace Vennu.Data.Services;

public interface IStripeEventIdempotencyService
{
    Task<bool> ExecuteOnceAsync(
        string eventId,
        string eventType,
        Func<CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}

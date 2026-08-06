using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class PosWebhookWorker(
    IServiceScopeFactory scopeFactory,
    IPosWebhookWorkSignal signal,
    TimeProvider timeProvider,
    ILogger<PosWebhookWorker> logger) : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan ProcessingLease = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var processedWork = false;
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IPosWebhookEventRepository>();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IPosWebhookEventDispatcher>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    var now = timeProvider.GetUtcNow().UtcDateTime;
                    var webhookEvent = await repository.TryClaimNextAsync(now, now.Subtract(ProcessingLease), stoppingToken).ConfigureAwait(false);
                    if (webhookEvent is null) break;
                    processedWork = true;
                    try
                    {
                        await dispatcher.DispatchAsync(webhookEvent, stoppingToken).ConfigureAwait(false);
                        if (!await repository.MarkProcessedAsync(webhookEvent.Id, timeProvider.GetUtcNow().UtcDateTime, stoppingToken).ConfigureAwait(false))
                            throw new InvalidOperationException("The POS webhook event lost its processing claim.");
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                    catch (Exception exception)
                    {
                        var failedUtc = timeProvider.GetUtcNow().UtcDateTime;
                        var delayMinutes = Math.Min(15, Math.Max(1, webhookEvent.AttemptCount));
                        var reason = exception.Message.Length <= 500 ? exception.Message : exception.Message[..500];
                        await repository.MarkFailedAsync(webhookEvent.Id, reason, failedUtc, failedUtc.AddMinutes(delayMinutes), CancellationToken.None).ConfigureAwait(false);
                        logger.LogWarning("POS webhook {Provider}/{EventId} attempt {Attempt} failed and will retry.", webhookEvent.Provider, webhookEvent.ProviderEventId, webhookEvent.AttemptCount);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception)
            {
                // Claiming work, resolving scopes and signalling all sit outside the
                // per-event handler above. Letting any of them escape ExecuteAsync takes
                // the whole API host down with it, because the default
                // BackgroundServiceExceptionBehavior is StopHost. A POS polling failure
                // must never stop the API from serving requests.
                logger.LogError(exception, "The POS webhook worker cycle failed and will retry after the idle delay.");
                processedWork = false;
            }

            if (processedWork) continue;
            try { await signal.WaitAsync(IdleDelay, stoppingToken).ConfigureAwait(false); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}

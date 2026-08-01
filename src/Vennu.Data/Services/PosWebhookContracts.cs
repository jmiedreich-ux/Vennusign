using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed record VerifiedPosWebhookEvent(
    PosProvider Provider,
    string ProviderEventId,
    string EventType,
    string ExternalMerchantId,
    string Payload);

public interface IPosWebhookEventHandler
{
    bool CanHandle(PosProvider provider, string eventType);
    Task HandleAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}

public interface IPosWebhookEventDispatcher
{
    Task DispatchAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}

public sealed class PosWebhookEventDispatcher(IEnumerable<IPosWebhookEventHandler> handlers) : IPosWebhookEventDispatcher
{
    public async Task DispatchAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        foreach (var handler in handlers.Where(value => value.CanHandle(webhookEvent.Provider, webhookEvent.EventType)))
            await handler.HandleAsync(webhookEvent, cancellationToken).ConfigureAwait(false);
    }
}

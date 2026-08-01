using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class PosWebhookEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_InvokesOnlyMatchingProviderNeutralHandlers()
    {
        var matching = new HandlerFake(true);
        var other = new HandlerFake(false);
        var dispatcher = new PosWebhookEventDispatcher([matching, other]);

        await dispatcher.DispatchAsync(new PosWebhookEvent { Provider = PosProvider.Square, EventType = "inventory.count.updated" });

        Assert.Equal(1, matching.Count);
        Assert.Equal(0, other.Count);
    }

    private sealed class HandlerFake(bool matches) : IPosWebhookEventHandler
    {
        public int Count { get; private set; }
        public bool CanHandle(PosProvider provider, string eventType) => matches;
        public Task HandleAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default) { Count++; return Task.CompletedTask; }
    }
}

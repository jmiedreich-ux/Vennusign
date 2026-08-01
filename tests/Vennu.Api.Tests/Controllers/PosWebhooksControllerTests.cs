using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers;
using Vennu.Api.Contracts.Webhooks;
using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class PosWebhooksControllerTests
{
    [Fact]
    public async Task Receive_VerifiesPersistsSignalsAndReturnsAccepted()
    {
        var repository = new RepositoryFake { Enqueued = true };
        var signal = new SignalFake();
        var controller = Create(new VerifierFake(), repository, signal, "signature", "{}");

        var result = await controller.Receive("square", CancellationToken.None);

        var accepted = Assert.IsType<AcceptedResult>(result.Result);
        Assert.True(Assert.IsType<PosWebhookResponse>(accepted.Value).Queued);
        Assert.Equal("event-1", repository.Value?.ProviderEventId);
        Assert.True(signal.Signaled);
    }

    [Fact]
    public async Task Receive_DuplicateStillReturnsAcceptedWithoutSignal()
    {
        var repository = new RepositoryFake { Enqueued = false };
        var signal = new SignalFake();
        var controller = Create(new VerifierFake(), repository, signal, "signature", "{}");

        var result = await controller.Receive("square", CancellationToken.None);

        var accepted = Assert.IsType<AcceptedResult>(result.Result);
        Assert.False(Assert.IsType<PosWebhookResponse>(accepted.Value).Queued);
        Assert.False(signal.Signaled);
    }

    [Theory]
    [InlineData("unknown", "signature")]
    [InlineData("square", "")]
    public async Task Receive_RejectsInvalidProviderOrMissingSignature(string provider, string signature)
    {
        var controller = Create(new VerifierFake(), new RepositoryFake(), new SignalFake(), signature, "{}");

        var result = await controller.Receive(provider, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    private static PosWebhooksController Create(IPosWebhookVerifier verifier, RepositoryFake repository, SignalFake signal, string signature, string payload)
    {
        var controller = new PosWebhooksController([verifier], repository, signal);
        var context = new DefaultHttpContext();
        context.Request.Headers[verifier.SignatureHeaderName] = signature;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    private sealed class VerifierFake : IPosWebhookVerifier
    {
        public PosProvider Provider => PosProvider.Square;
        public string SignatureHeaderName => "test-signature";
        public VerifiedPosWebhookEvent Verify(string payload, string signature) => new(Provider, "event-1", "inventory.count.updated", "merchant-1", payload);
    }

    private sealed class RepositoryFake : IPosWebhookEventRepository
    {
        public bool Enqueued { get; init; }
        public PosWebhookEvent? Value { get; private set; }
        public Task<bool> EnqueueAsync(PosWebhookEvent webhookEvent, CancellationToken cancellationToken = default) { Value = webhookEvent; return Task.FromResult(Enqueued); }
        public Task<PosWebhookEvent?> TryClaimNextAsync(DateTime utcNow, DateTime staleBeforeUtc, CancellationToken cancellationToken = default) => Task.FromResult<PosWebhookEvent?>(null);
        public Task<bool> MarkProcessedAsync(Guid id, DateTime processedUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> MarkFailedAsync(Guid id, string failureReason, DateTime failedUtc, DateTime nextAttemptUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class SignalFake : IPosWebhookWorkSignal
    {
        public bool Signaled { get; private set; }
        public void Signal() => Signaled = true;
        public Task WaitAsync(TimeSpan maximumDelay, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

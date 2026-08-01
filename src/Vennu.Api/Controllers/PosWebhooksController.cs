using System.Text;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.Webhooks;
using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/webhooks/pos/{provider}")]
public sealed class PosWebhooksController(
    IEnumerable<IPosWebhookVerifier> verifiers,
    IPosWebhookEventRepository repository,
    IPosWebhookWorkSignal signal) : ControllerBase
{
    private const int MaximumPayloadBytes = 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaximumPayloadBytes)]
    [ProducesResponseType<PosWebhookResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PosWebhookResponse>> Receive(string provider, CancellationToken cancellationToken)
    {
        if (!TryProvider(provider, out var providerValue)) return InvalidWebhook();
        var verifier = verifiers.SingleOrDefault(value => value.Provider == providerValue);
        if (verifier is null) return InvalidWebhook();
        var signature = Request.Headers[verifier.SignatureHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(signature)) return InvalidWebhook();

        VerifiedPosWebhookEvent verified;
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, false, leaveOpen: true);
            var payload = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            if (Encoding.UTF8.GetByteCount(payload) > MaximumPayloadBytes) return InvalidWebhook();
            verified = verifier.Verify(payload, signature);
        }
        catch (Exception exception) when (exception is PosWebhookVerificationException or InvalidOperationException or ArgumentException)
        {
            return InvalidWebhook();
        }

        var queued = await repository.EnqueueAsync(new PosWebhookEvent
        {
            Provider = verified.Provider,
            ProviderEventId = verified.ProviderEventId,
            EventType = verified.EventType,
            ExternalMerchantId = verified.ExternalMerchantId,
            Payload = verified.Payload,
            Status = PosWebhookEventStatus.Queued
        }, cancellationToken).ConfigureAwait(false);
        if (queued) signal.Signal();
        return Accepted(new PosWebhookResponse(Received: true, Queued: queued));
    }

    private BadRequestObjectResult InvalidWebhook() => BadRequest(new ProblemDetails
    {
        Title = "Invalid POS webhook.",
        Detail = "The provider, signature, or payload is invalid.",
        Status = StatusCodes.Status400BadRequest
    });

    private static bool TryProvider(string value, out PosProvider provider)
    {
        provider = value.Trim().ToLowerInvariant() switch
        {
            "square" => PosProvider.Square,
            "toast" => PosProvider.Toast,
            "clover" => PosProvider.Clover,
            _ => default
        };
        return provider != default;
    }
}

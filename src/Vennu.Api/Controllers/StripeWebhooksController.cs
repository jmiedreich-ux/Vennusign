using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Vennu.Api.Contracts.Webhooks;
using Vennu.Api.Webhooks;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public sealed class StripeWebhooksController : ControllerBase
{
    private const int MaximumPayloadBytes = 1024 * 1024;
    private readonly IStripeWebhookEventVerifier verifier;
    private readonly IStripeSubscriptionEventHandler eventHandler;
    private readonly IHaasContractSubscriptionEventHandler haasEventHandler;

    public StripeWebhooksController(
        IStripeWebhookEventVerifier verifier,
        IStripeSubscriptionEventHandler eventHandler,
        IHaasContractSubscriptionEventHandler haasEventHandler)
    {
        this.verifier = verifier;
        this.eventHandler = eventHandler;
        this.haasEventHandler = haasEventHandler;
    }

    [HttpPost]
    [RequestSizeLimit(MaximumPayloadBytes)]
    [ProducesResponseType<StripeWebhookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StripeWebhookResponse>> Receive(
        CancellationToken cancellationToken)
    {
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return InvalidWebhook();
        }

        Stripe.Event stripeEvent;
        StripeSubscriptionEvent? subscriptionEvent;
        HaasContractSubscriptionEvent? haasEvent;
        try
        {
            using var reader = new StreamReader(
                Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            var payload = await reader.ReadToEndAsync(cancellationToken);
            stripeEvent = verifier.Verify(payload, signatureHeader);

            if (StripeHaasWebhookEventMapper.TryMap(stripeEvent, out haasEvent))
            {
                var haasProcessed = await haasEventHandler
                    .HandleAsync(haasEvent!, cancellationToken)
                    .ConfigureAwait(false);
                return Ok(new StripeWebhookResponse(Received: true, Processed: haasProcessed));
            }

            if (!StripeWebhookEventMapper.TryMap(stripeEvent, out subscriptionEvent))
            {
                return Ok(new StripeWebhookResponse(Received: true, Processed: false));
            }
        }
        catch (Exception exception) when (
            exception is StripeException or
            JsonException or
            ArgumentException or
            StripeWebhookPayloadException)
        {
            return InvalidWebhook();
        }

        var processed = await eventHandler
            .HandleAsync(subscriptionEvent!, cancellationToken)
            .ConfigureAwait(false);

        return Ok(new StripeWebhookResponse(Received: true, Processed: processed));
    }

    private BadRequestObjectResult InvalidWebhook() =>
        BadRequest(new ProblemDetails
        {
            Title = "Invalid Stripe webhook.",
            Detail = "The webhook signature or payload is invalid.",
            Status = StatusCodes.Status400BadRequest
        });
}

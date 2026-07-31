using System.Text;
using Stripe;
using Vennu.Api.Contracts.Webhooks;
using Vennu.Api.Controllers;
using Vennu.Api.Webhooks;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class StripeWebhooksControllerTests
{
    [Fact]
    public async Task Receive_ReturnsBadRequest_WhenSignatureHeaderIsMissing()
    {
        var handler = new RecordingHandler();
        var sut = CreateController(new StubVerifier(), handler, signatureHeader: null);

        var result = await sut.Receive(CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        Assert.Null(handler.LastEvent);
    }

    [Fact]
    public async Task Receive_ReturnsBadRequest_WhenSignatureIsInvalid()
    {
        var handler = new RecordingHandler();
        var sut = CreateController(
            new StubVerifier { Exception = new StripeException("invalid signature") },
            handler);

        var result = await sut.Receive(CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        Assert.Null(handler.LastEvent);
    }

    [Fact]
    public async Task Receive_AcknowledgesUnsupportedVerifiedEvent()
    {
        var handler = new RecordingHandler();
        var sut = CreateController(
            new StubVerifier
            {
                Event = new Event
                {
                    Id = "evt_unsupported",
                    Type = EventTypes.CustomerCreated
                }
            },
            handler);

        var result = await sut.Receive(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<StripeWebhookResponse>(ok.Value);
        Assert.True(response.Received);
        Assert.False(response.Processed);
        Assert.Null(handler.LastEvent);
    }

    [Fact]
    public async Task Receive_DispatchesSupportedVerifiedEvent()
    {
        var handler = new RecordingHandler { Result = true };
        var sut = CreateController(
            new StubVerifier
            {
                Event = new Event
                {
                    Id = "evt_deleted",
                    Type = EventTypes.CustomerSubscriptionDeleted,
                    Data = new EventData
                    {
                        Object = new Subscription { Id = "sub_deleted" }
                    }
                }
            },
            handler);

        var result = await sut.Receive(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<StripeWebhookResponse>(ok.Value);
        Assert.True(response.Received);
        Assert.True(response.Processed);
        Assert.NotNull(handler.LastEvent);
        Assert.Equal("evt_deleted", handler.LastEvent.EventId);
        Assert.Equal("sub_deleted", handler.LastEvent.StripeSubscriptionId);
    }

    [Fact]
    public async Task Receive_ReturnsBadRequest_WhenSupportedPayloadIsMalformed()
    {
        var handler = new RecordingHandler();
        var sut = CreateController(
            new StubVerifier
            {
                Event = new Event
                {
                    Id = "evt_bad",
                    Type = EventTypes.InvoicePaid,
                    Data = new EventData
                    {
                        Object = new Invoice { Id = "in_bad" }
                    }
                }
            },
            handler);

        var result = await sut.Receive(CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        Assert.Null(handler.LastEvent);
    }

    private static StripeWebhooksController CreateController(
        IStripeWebhookEventVerifier verifier,
        RecordingHandler handler,
        string? signatureHeader = "t=123,v1=signature")
    {
        var controller = new StripeWebhooksController(verifier, handler, new RecordingHaasHandler())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        if (signatureHeader is not null)
        {
            controller.Request.Headers["Stripe-Signature"] = signatureHeader;
        }

        return controller;
    }

    private sealed class StubVerifier : IStripeWebhookEventVerifier
    {
        public Event? Event { get; init; }

        public Exception? Exception { get; init; }

        public Event Verify(string payload, string signatureHeader)
        {
            var exception = Exception;
            if (exception is not null)
            {
                throw exception;
            }

            return Event ?? new Event { Id = "evt_default", Type = EventTypes.CustomerCreated };
        }
    }

    private sealed class RecordingHandler : IStripeSubscriptionEventHandler
    {
        public StripeSubscriptionEvent? LastEvent { get; private set; }

        public bool Result { get; init; }

        public Task<bool> HandleAsync(
            StripeSubscriptionEvent stripeEvent,
            CancellationToken cancellationToken = default)
        {
            LastEvent = stripeEvent;
            return Task.FromResult(Result);
        }
    }

    private sealed class RecordingHaasHandler : IHaasContractSubscriptionEventHandler
    {
        public Task<bool> HandleAsync(
            HaasContractSubscriptionEvent stripeEvent,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }
}

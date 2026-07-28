using Microsoft.Extensions.Options;
using Stripe;
using Vennu.Api.Webhooks;

namespace Vennu.Api.Tests.Webhooks;

[Trait("Category", "Unit")]
public sealed class StripeWebhookEventVerifierTests
{
    private const string SigningSecret = "whsec_unit_test_secret";
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 28, 20, 30, 0, TimeSpan.Zero);
    private const string Payload = """
        {
          "id": "evt_verified",
          "object": "event",
          "api_version": "2026-06-24.dahlia",
          "created": 1785270600,
          "livemode": false,
          "pending_webhooks": 1,
          "type": "customer.subscription.deleted",
          "data": {
            "object": {
              "id": "sub_verified",
              "object": "subscription"
            }
          }
        }
        """;

    [Fact]
    public void Verify_ReturnsEvent_WhenSignatureIsValidAndCurrent()
    {
        var sut = CreateVerifier();
        var timestamp = UtcNow.ToUnixTimeSeconds().ToString();
        var signature = EventUtility.ComputeSignature(SigningSecret, timestamp, Payload);

        var result = sut.Verify(Payload, $"t={timestamp},v1={signature}");

        Assert.Equal("evt_verified", result.Id);
        Assert.Equal(EventTypes.CustomerSubscriptionDeleted, result.Type);
    }

    [Fact]
    public void Verify_Throws_WhenSignatureTimestampIsStale()
    {
        var sut = CreateVerifier();
        var timestamp = UtcNow.AddMinutes(-6).ToUnixTimeSeconds().ToString();
        var signature = EventUtility.ComputeSignature(SigningSecret, timestamp, Payload);

        Assert.Throws<StripeException>(() =>
            sut.Verify(Payload, $"t={timestamp},v1={signature}"));
    }

    [Fact]
    public void Verify_Throws_WhenSignatureDoesNotMatch()
    {
        var sut = CreateVerifier();
        var timestamp = UtcNow.ToUnixTimeSeconds().ToString();

        Assert.Throws<StripeException>(() =>
            sut.Verify(Payload, $"t={timestamp},v1=invalid"));
    }

    private static StripeWebhookEventVerifier CreateVerifier() =>
        new(
            Options.Create(new StripeWebhookOptions
            {
                SigningSecret = SigningSecret,
                ToleranceSeconds = 300
            }),
            new FixedTimeProvider(UtcNow));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

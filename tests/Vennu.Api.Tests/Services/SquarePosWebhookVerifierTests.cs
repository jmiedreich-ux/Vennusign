using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class SquarePosWebhookVerifierTests
{
    private const string NotificationUrl = "https://api.vennu.com/api/webhooks/pos/square";
    private const string SignatureKey = "test-signature-key";
    private const string Payload = "{\"event_id\":\"event-1\",\"type\":\"inventory.count.updated\",\"merchant_id\":\"merchant-1\"}";

    [Fact]
    public void Verify_AcceptsValidSignatureAndMapsProviderEnvelope()
    {
        var verifier = Create();

        var result = verifier.Verify(Payload, Signature(Payload));

        Assert.Equal("event-1", result.ProviderEventId);
        Assert.Equal("inventory.count.updated", result.EventType);
        Assert.Equal("merchant-1", result.ExternalMerchantId);
        Assert.Equal(Payload, result.Payload);
    }

    [Fact]
    public void Verify_RejectsTamperingAndMissingMetadata()
    {
        var verifier = Create();

        Assert.Throws<PosWebhookVerificationException>(() => verifier.Verify(Payload + " ", Signature(Payload)));
        const string missing = "{\"event_id\":\"event-1\"}";
        Assert.Throws<PosWebhookVerificationException>(() => verifier.Verify(missing, Signature(missing)));
    }

    private static SquarePosWebhookVerifier Create() => new(Options.Create(new SquareWebhookOptions
    {
        NotificationUrl = NotificationUrl,
        SignatureKey = SignatureKey
    }));

    private static string Signature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SignatureKey));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(NotificationUrl + payload)));
    }
}

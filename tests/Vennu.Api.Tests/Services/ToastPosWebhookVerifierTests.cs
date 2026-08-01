using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Vennu.Api.Pos;
using Vennu.Core.Models;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class ToastPosWebhookVerifierTests
{
    private const string Secret = "toast-stock-secret";
    private const string Timestamp = "2026-08-01T02:10:00.000Z";
    private const string Payload = "{\"timestamp\":\"2026-08-01T02:10:00.000Z\",\"eventCategory\":\"stock\",\"eventType\":\"low_quantity\",\"guid\":\"e445f586-081c-4a2a-bcd6-30717a48e17a\",\"details\":{\"restaurantGuid\":\"3325cc58-dc6e-4e21-85f9-7de275ffe820\"}}";

    [Fact]
    public void Verify_AcceptsToastSignatureAndCanonicalGuids()
    {
        var result = Create().Verify(Payload, Signature(Payload));

        Assert.Equal(PosProvider.Toast, result.Provider);
        Assert.Equal("e445f586-081c-4a2a-bcd6-30717a48e17a", result.ProviderEventId);
        Assert.Equal("low_quantity", result.EventType);
        Assert.Equal("3325cc58-dc6e-4e21-85f9-7de275ffe820", result.ExternalMerchantId);
    }

    [Fact]
    public void Verify_RejectsTamperingAndNonGuidEventId()
    {
        Assert.Throws<PosWebhookVerificationException>(() => Create().Verify(Payload + " ", Signature(Payload)));
        var invalid = Payload.Replace("e445f586-081c-4a2a-bcd6-30717a48e17a", "event-1", StringComparison.Ordinal);
        Assert.Throws<PosWebhookVerificationException>(() => Create().Verify(invalid, Signature(invalid)));
    }

    private static ToastPosWebhookVerifier Create() => new(Options.Create(new ToastWebhookOptions
    {
        StockSecret = Secret,
        MenusSecret = "toast-menus-secret"
    }));

    private static string Signature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload + Timestamp)));
    }
}

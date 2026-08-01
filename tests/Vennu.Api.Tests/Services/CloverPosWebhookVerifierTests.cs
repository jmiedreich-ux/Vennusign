using Microsoft.Extensions.Options;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class CloverPosWebhookVerifierTests
{
    [Fact]
    public void VerifyMany_AuthenticatesAndExpandsMerchantInventoryEventsDeterministically()
    {
        var verifier = Create();
        var payload = """
            {
              "appId": "clover-app",
              "merchants": {
                "merchant-1": [
                  { "objectId": "I:item-1", "type": "UPDATE", "ts": 1785556800000 },
                  { "objectId": "IC:category-1", "type": "UPDATE", "ts": 1785556800001 }
                ],
                "merchant-2": [
                  { "objectId": "I:item-2", "type": "DELETE", "ts": 1785556800002 }
                ]
              }
            }
            """;

        var first = verifier.VerifyMany(payload, "clover-auth");
        var replay = verifier.VerifyMany(payload, "clover-auth");

        Assert.Equal(2, first.Count);
        Assert.Equal(first.Select(value => value.ProviderEventId), replay.Select(value => value.ProviderEventId));
        Assert.Equal(["merchant-1", "merchant-2"], first.Select(value => value.ExternalMerchantId));
        Assert.Equal(["inventory.item.update", "inventory.item.delete"], first.Select(value => value.EventType));
        Assert.All(first, value => Assert.DoesNotContain("category-1", value.Payload, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("wrong-auth", "clover-app")]
    [InlineData("clover-auth", "wrong-app")]
    public void VerifyMany_RejectsWrongAuthOrApplication(string authCode, string appId)
    {
        var verifier = Create();
        var payload = $$"""{ "appId": "{{appId}}", "merchants": { "merchant-1": [{ "objectId": "I:item-1", "type": "UPDATE", "ts": 1 }] } }""";

        Assert.Throws<PosWebhookVerificationException>(() => verifier.VerifyMany(payload, authCode));
    }

    private static CloverPosWebhookVerifier Create() => new(Options.Create(new CloverWebhookOptions
    {
        AppId = "clover-app",
        AuthCode = "clover-auth"
    }));
}

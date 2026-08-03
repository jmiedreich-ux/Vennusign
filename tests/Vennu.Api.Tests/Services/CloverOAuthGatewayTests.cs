using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class CloverOAuthGatewayTests
{
    [Fact]
    public void CreateAuthorizationUri_UsesV2OfficialHostAndCorrelatedState()
    {
        var gateway = CreateGateway(new RecordingHandler(HttpStatusCode.OK, "{}"));

        var uri = gateway.CreateAuthorizationUri("protected-state");

        Assert.Equal("www.clover.com", uri.Host);
        Assert.Equal("/oauth/v2/authorize", uri.AbsolutePath);
        Assert.Contains("client_id=clover-client", uri.Query, StringComparison.Ordinal);
        Assert.Contains("state=protected-state", uri.Query, StringComparison.Ordinal);
        Assert.Contains("redirect_uri=https%3A%2F%2Fapi.vennu.test%2Fapi%2Fback-office%2Fpos%2Fclover%2Fcallback", uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExchangeCodeAsync_UsesOfficialTokenHostAndDynamicExpirations()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK, """
            {
              "access_token": "access-secret",
              "refresh_token": "refresh-secret",
              "access_token_expiration": 1785556800,
              "refresh_token_expiration": 1817092800
            }
            """);
        var gateway = CreateGateway(handler);

        var result = await gateway.ExchangeCodeAsync("authorization-code");

        Assert.Equal("api.clover.com", handler.RequestUri!.Host);
        Assert.Equal("/oauth/v2/token", handler.RequestUri.AbsolutePath);
        Assert.Contains("authorization-code", handler.Body, StringComparison.Ordinal);
        Assert.Equal("access-secret", result.AccessToken);
        Assert.Equal(DateTimeKind.Utc, result.AccessTokenExpiresUtc.Kind);
        Assert.True(result.RefreshTokenExpiresUtc > result.AccessTokenExpiresUtc);
    }

    [Fact]
    public void CreateAuthorizationUri_RejectsLookalikeHost()
    {
        var options = Options.Create(ValidOptions());
        options.Value.AuthorizationEndpoint = "https://www.clover.com.attacker.test/oauth/v2/authorize";
        var gateway = new CloverOAuthGateway(new HttpClient(new RecordingHandler(HttpStatusCode.OK, "{}")), options);

        Assert.Throws<InvalidOperationException>(() => gateway.CreateAuthorizationUri("state"));
    }

    [Fact]
    public void ValidateClientId_RejectsDifferentApplication()
    {
        var gateway = CreateGateway(new RecordingHandler(HttpStatusCode.OK, "{}"));

        Assert.Throws<InvalidOperationException>(() => gateway.ValidateClientId("different-client"));
    }

    private static CloverOAuthGateway CreateGateway(HttpMessageHandler handler) =>
        new(new HttpClient(handler), Options.Create(ValidOptions()));

    private static CloverOAuthOptions ValidOptions() => new()
    {
        ClientId = "clover-client",
        ClientSecret = "clover-secret",
        CallbackUrl = "https://api.vennu.test/api/back-office/pos/clover/callback",
        BackOfficeReturnUrl = "https://app.vennu.test/integrations"
    };

    private sealed class RecordingHandler(HttpStatusCode status, string responseBody) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}

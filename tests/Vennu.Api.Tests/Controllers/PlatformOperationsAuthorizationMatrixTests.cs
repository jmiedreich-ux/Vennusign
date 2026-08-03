using System.Net;
using System.Text;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class PlatformOperationsAuthorizationMatrixTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public PlatformOperationsAuthorizationMatrixTests(VennuApiFactory factory) => this.factory = factory;

    public static TheoryData<string, string> ProtectedEndpoints =>
        new()
        {
            { "GET", "/api/platform-operations/session" },
            { "GET", "/api/platform-operations/dashboard" },
            { "GET", "/api/platform-operations/dashboard/events" },
            { "GET", "/api/platform-operations/dashboard/revenue" },
            { "GET", "/api/platform-operations/dashboard/revenue/trend" },
            { "GET", "/api/platform-operations/venues" },
            { "GET", $"/api/platform-operations/venues/{Guid.NewGuid()}" },
            { "PUT", $"/api/platform-operations/venues/{Guid.NewGuid()}/tier" },
            { "PUT", $"/api/platform-operations/venues/{Guid.NewGuid()}/overrides/{Guid.NewGuid()}" },
            { "DELETE", $"/api/platform-operations/venues/{Guid.NewGuid()}/overrides/{Guid.NewGuid()}" },
            { "GET", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens" },
            { "POST", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens" },
            { "PUT", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens/{Guid.NewGuid()}" },
            { "POST", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens/{Guid.NewGuid()}/push" },
            { "POST", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens/push-all" },
            { "GET", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens/overflow?capacity=6" },
            { "GET", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens/video-walls" },
            { "PUT", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens/video-walls" },
            { "DELETE", $"/api/platform-operations/venues/{Guid.NewGuid()}/screens/video-walls/main" },
            { "POST", "/api/screens/pairing/123456/claim" },
            { "GET", $"/api/platform-operations/venues/{Guid.NewGuid()}/theme" },
            { "PUT", $"/api/platform-operations/venues/{Guid.NewGuid()}/theme" },
            { "GET", "/api/platform-operations/tiers" },
            { "POST", "/api/platform-operations/tiers" },
            { "PUT", $"/api/platform-operations/tiers/{Guid.NewGuid()}" },
            { "POST", $"/api/platform-operations/tiers/{Guid.NewGuid()}/clone" },
            { "POST", $"/api/platform-operations/tiers/{Guid.NewGuid()}/archive" },
            { "GET", "/api/platform-operations/features" },
            { "PUT", "/api/platform-operations/features" }
        };

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoint_ReturnsUnauthorized_WhenAdminKeyIsMissing(
        string method,
        string path)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method is "POST" or "PUT")
        {
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        }

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

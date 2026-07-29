using System.Net;
using System.Text;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class SuperAdminAuthorizationMatrixTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public SuperAdminAuthorizationMatrixTests(VennuApiFactory factory) => this.factory = factory;

    public static TheoryData<string, string> ProtectedEndpoints =>
        new()
        {
            { "GET", "/api/admin/session" },
            { "GET", "/api/admin/dashboard" },
            { "GET", "/api/admin/dashboard/events" },
            { "GET", "/api/admin/dashboard/revenue" },
            { "GET", "/api/admin/dashboard/revenue/trend" },
            { "GET", "/api/admin/venues" },
            { "GET", $"/api/admin/venues/{Guid.NewGuid()}" },
            { "PUT", $"/api/admin/venues/{Guid.NewGuid()}/tier" },
            { "PUT", $"/api/admin/venues/{Guid.NewGuid()}/overrides/{Guid.NewGuid()}" },
            { "DELETE", $"/api/admin/venues/{Guid.NewGuid()}/overrides/{Guid.NewGuid()}" },
            { "GET", "/api/admin/tiers" },
            { "POST", "/api/admin/tiers" },
            { "PUT", $"/api/admin/tiers/{Guid.NewGuid()}" },
            { "POST", $"/api/admin/tiers/{Guid.NewGuid()}/clone" },
            { "POST", $"/api/admin/tiers/{Guid.NewGuid()}/archive" },
            { "GET", "/api/admin/features" },
            { "PUT", "/api/admin/features" }
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

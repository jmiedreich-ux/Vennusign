using System.Net;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class VenueAdminSquarePosControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public VenueAdminSquarePosControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Theory]
    [InlineData("GET", "/api/venue-admin/pos/square/status")]
    [InlineData("POST", "/api/venue-admin/pos/square/connect")]
    [InlineData("DELETE", "/api/venue-admin/pos/square/connection")]
    public async Task VenueOperations_RequireVenueAdminAuthentication(string method, string path)
    {
        using var client = factory.CreateClient();
        using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

using System.Net;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class VenueAdminCloverPosControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public VenueAdminCloverPosControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Theory]
    [InlineData("GET", "/api/venue-admin/pos/clover/status")]
    [InlineData("POST", "/api/venue-admin/pos/clover/connect")]
    [InlineData("POST", "/api/venue-admin/pos/clover/catalog/import")]
    [InlineData("DELETE", "/api/venue-admin/pos/clover/connection")]
    public async Task VenueOperations_RequireVenueAdminAuthentication(string method, string path)
    {
        using var client = factory.CreateClient();
        using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

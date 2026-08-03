using System.Net;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeCloverPosControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public BackOfficeCloverPosControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Theory]
    [InlineData("GET", "/api/back-office/pos/clover/status")]
    [InlineData("POST", "/api/back-office/pos/clover/connect")]
    [InlineData("POST", "/api/back-office/pos/clover/catalog/import")]
    [InlineData("DELETE", "/api/back-office/pos/clover/connection")]
    public async Task VenueOperations_RequireBackOfficeAuthentication(string method, string path)
    {
        using var client = factory.CreateClient();
        using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

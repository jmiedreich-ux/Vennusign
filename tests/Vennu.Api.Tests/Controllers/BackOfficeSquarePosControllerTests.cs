using System.Net;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeSquarePosControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public BackOfficeSquarePosControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Theory]
    [InlineData("GET", "/api/back-office/pos/square/status")]
    [InlineData("POST", "/api/back-office/pos/square/connect")]
    [InlineData("POST", "/api/back-office/pos/square/catalog/import")]
    [InlineData("DELETE", "/api/back-office/pos/square/connection")]
    public async Task VenueOperations_RequireBackOfficeAuthentication(string method, string path)
    {
        using var client = factory.CreateClient();
        using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

using System.Net;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeOperationalControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public BackOfficeOperationalControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Operations_ReturnUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/back-office/venues/11111111-1111-1111-1111-111111111111/screens");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Operations_ForbidAccessToAnotherVenue()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Back-Office-Token", "test-venue-token");

        var response = await client.GetAsync(
            $"/api/back-office/venues/{Guid.NewGuid()}/screens");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Pairing_RejectsPlatformOperationsCredentials()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Platform-Operations-Key", "test-admin-key");

        var response = await client.PostAsync(
            "/api/back-office/screens/pairing/123456/claim",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

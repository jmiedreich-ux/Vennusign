using System.Net;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class VenueAdminOperationalControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public VenueAdminOperationalControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Operations_ReturnUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/venue-admin/venues/11111111-1111-1111-1111-111111111111/screens");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Operations_ForbidAccessToAnotherVenue()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Venue-Token", "test-venue-token");

        var response = await client.GetAsync(
            $"/api/venue-admin/venues/{Guid.NewGuid()}/screens");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Pairing_RejectsSuperAdminCredentials()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Admin-Key", "test-admin-key");

        var response = await client.PostAsync(
            "/api/venue-admin/screens/pairing/123456/claim",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

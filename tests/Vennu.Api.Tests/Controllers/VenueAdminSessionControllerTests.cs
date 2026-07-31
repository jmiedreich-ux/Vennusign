using System.Net;
using System.Net.Http.Json;
using Vennu.Api.Contracts.VenueAdmin;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class VenueAdminSessionControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public VenueAdminSessionControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/venue-admin/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenSuperAdminKeyIsUsed()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Admin-Key", "test-admin-key");

        var response = await client.GetAsync("/api/venue-admin/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsVenueScopedBootstrap_WhenVenueTokenIsValid()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Venue-Token", "test-venue-token");

        var response = await client.GetAsync("/api/venue-admin/session");

        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<VenueAdminSessionResponse>();
        Assert.NotNull(session);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), session.VenueId);
        Assert.Equal("Harbor Owner", session.DisplayName);
        Assert.Equal(["menus", "screens"], session.Capabilities);
    }
}

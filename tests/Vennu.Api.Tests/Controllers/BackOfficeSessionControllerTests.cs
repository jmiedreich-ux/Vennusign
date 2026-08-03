using System.Net;
using System.Net.Http.Json;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeSessionControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public BackOfficeSessionControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/back-office/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenPlatformOperationsKeyIsUsed()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Platform-Operations-Key", "test-admin-key");

        var response = await client.GetAsync("/api/back-office/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsVenueScopedBootstrap_WhenVenueTokenIsValid()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Back-Office-Token", "test-venue-token");

        var response = await client.GetAsync("/api/back-office/session");

        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<BackOfficeSessionResponse>();
        Assert.NotNull(session);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), session.VenueId);
        Assert.Equal("Harbor Owner", session.DisplayName);
        Assert.Equal(["menus", "screens"], session.Capabilities);
    }

    [Fact]
    public async Task Get_AcceptsLegacyRouteAndHeader_DuringMigrationWindow()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Venue-Token", "test-venue-token");

        var response = await client.GetAsync("/api/venue-admin/session");

        response.EnsureSuccessStatusCode();
        Assert.Equal("true", response.Headers.GetValues("Deprecation").Single());
    }

    [Fact]
    public async Task Get_RejectsConflictingCanonicalAndLegacyTokens()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Back-Office-Token", "test-venue-token");
        client.DefaultRequestHeaders.Add("X-Vennu-Venue-Token", "different-token");

        var response = await client.GetAsync("/api/back-office/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

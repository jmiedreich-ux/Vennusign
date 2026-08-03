using System.Net;
using System.Net.Http.Json;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class PlatformOperationsSessionControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public PlatformOperationsSessionControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenAdminKeyIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/platform-operations/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsCapabilities_WhenAdminKeyIsValid()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Platform-Operations-Key", "test-admin-key");

        var response = await client.GetAsync("/api/platform-operations/session");

        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<PlatformOperationsSessionResponse>();
        Assert.NotNull(session);
        Assert.Equal("Platform Operations", session.DisplayName);
        Assert.Contains("venues", session.Capabilities);
    }

    [Fact]
    public async Task Get_AcceptsLegacyRouteAndHeader_DuringMigrationWindow()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Admin-Key", "test-admin-key");

        var response = await client.GetAsync("/api/admin/session");

        response.EnsureSuccessStatusCode();
        Assert.Equal("true", response.Headers.GetValues("Deprecation").Single());
    }

    [Fact]
    public async Task Get_RejectsConflictingCanonicalAndLegacyKeys()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Platform-Operations-Key", "test-admin-key");
        client.DefaultRequestHeaders.Add("X-Vennu-Admin-Key", "different-key");

        var response = await client.GetAsync("/api/platform-operations/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

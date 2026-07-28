using System.Net;
using System.Net.Http.Json;
using Vennu.Api.Contracts.Admin;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class SuperAdminSessionControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public SuperAdminSessionControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenAdminKeyIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsCapabilities_WhenAdminKeyIsValid()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Admin-Key", "test-admin-key");

        var response = await client.GetAsync("/api/admin/session");

        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<SuperAdminSessionResponse>();
        Assert.NotNull(session);
        Assert.Equal("Super Admin", session.DisplayName);
        Assert.Contains("venues", session.Capabilities);
    }
}

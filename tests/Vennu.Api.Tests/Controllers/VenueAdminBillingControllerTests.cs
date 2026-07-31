using System.Net;
using System.Net.Http.Json;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class VenueAdminBillingControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public VenueAdminBillingControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Presentation_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/venue-admin/billing/presentation");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Presentation_RejectsSuperAdminCredentials()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennu-Admin-Key", "test-admin-key");

        var response = await client.GetAsync("/api/venue-admin/billing/presentation");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CheckoutSession_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/venue-admin/billing/checkout-session",
            new { targetTierId = Guid.NewGuid(), billingInterval = "monthly" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PortalSession_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/venue-admin/billing/portal-session",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

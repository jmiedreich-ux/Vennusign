using System.Net;
using System.Net.Http.Json;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeBillingControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public BackOfficeBillingControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Presentation_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/back-office/billing/presentation");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Presentation_RejectsPlatformOperationsCredentials()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Vennusign-Platform-Operations-Key", "test-admin-key");

        var response = await client.GetAsync("/api/back-office/billing/presentation");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CheckoutSession_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/back-office/billing/checkout-session",
            new { targetTierId = Guid.NewGuid(), billingInterval = "monthly" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PortalSession_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/back-office/billing/portal-session",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HaasCheckoutSession_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/back-office/billing/haas-checkout-session",
            new { bundleKey = "starter_kit", termMonths = 18 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

namespace Vennu.DevControl.Tests;

public sealed class DevelopmentServiceCatalogTests
{
    [Fact]
    public void ApiAndVenueAdminUseHttpsForCustomerAuthentication()
    {
        var services = DevelopmentServiceCatalog.Create("C:\\repo");
        var api = Assert.Single(services, service => service.Name == "API");
        var venueAdmin = Assert.Single(services, service => service.Name == "Venue Admin");

        Assert.Equal(7138, api.Port);
        Assert.Equal("https://localhost:7138", api.Url);
        Assert.Contains("--launch-profile https", api.Arguments, StringComparison.Ordinal);
        Assert.Equal("https://localhost:5174/", venueAdmin.Url);
        Assert.Contains("VITE_VENNU_API_BASE_URL=https://localhost:7138", venueAdmin.Environment);
    }

    [Fact]
    public void EveryLocalClientUsesTheSecureApiOrigin()
    {
        var services = DevelopmentServiceCatalog.Create("C:\\repo");

        foreach (var service in services.Where(service => service.Name != "API"))
        {
            Assert.Contains(service.Environment, value => value.Contains("https://localhost:7138", StringComparison.Ordinal));
        }
    }
}

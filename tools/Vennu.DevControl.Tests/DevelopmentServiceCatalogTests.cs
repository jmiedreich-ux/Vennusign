namespace Vennu.DevControl.Tests;

public sealed class DevelopmentServiceCatalogTests
{
    [Fact]
    public void ApiAndBackOfficeUseHttpsForCustomerAuthentication()
    {
        var services = DevelopmentServiceCatalog.Create("C:\\repo");
        var api = Assert.Single(services, service => service.Name == "API");
        var backOffice = Assert.Single(services, service => service.Name == "Back Office");

        Assert.Equal(7138, api.Port);
        Assert.Equal("https://localhost:7138", api.Url);
        Assert.Contains("--launch-profile https", api.Arguments, StringComparison.Ordinal);
        Assert.Equal("https://localhost:5174/", backOffice.Url);
        Assert.Contains("VITE_VENNUSIGN_API_BASE_URL=https://localhost:7138", backOffice.Environment);
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

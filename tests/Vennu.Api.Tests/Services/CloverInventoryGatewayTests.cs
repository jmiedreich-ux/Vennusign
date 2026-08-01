using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class CloverInventoryGatewayTests
{
    [Fact]
    public void Map_CombinesProviderAvailabilityStockAndFixedPrice()
    {
        using var document = JsonDocument.Parse("""
            {
              "id": "item-1", "available": true, "price": 1425, "priceType": "FIXED",
              "itemStock": { "quantity": 3 }
            }
            """);

        var result = CloverInventoryGateway.Map(document.RootElement, "item-1", "USD");

        Assert.True(result.IsAvailable);
        Assert.Equal(3, result.QuantityAvailable);
        Assert.Equal(14.25m, result.Price);
        Assert.Equal("USD", result.CurrencyCode);
    }

    [Fact]
    public void Map_ZeroStockOverridesAvailableAndVariablePriceRemainsUnsupported()
    {
        using var document = JsonDocument.Parse("""
            {
              "id": "item-1", "available": true, "price": 1425, "priceType": "VARIABLE",
              "itemStock": { "quantity": 0 }
            }
            """);

        var result = CloverInventoryGateway.Map(document.RootElement, "item-1", "USD");

        Assert.False(result.IsAvailable);
        Assert.Equal(0, result.QuantityAvailable);
        Assert.Null(result.Price);
        Assert.Null(result.CurrencyCode);
    }

    [Fact]
    public void Map_RejectsDifferentReturnedItem()
    {
        using var document = JsonDocument.Parse("""{ "id": "item-other" }""");

        Assert.Throws<InvalidOperationException>(() => CloverInventoryGateway.Map(document.RootElement, "item-1", "USD"));
    }

    [Fact]
    public async Task GetInventoryAsync_RejectsLookalikeHostBeforeSending()
    {
        var gateway = new CloverInventoryGateway(
            new HttpClient(new RejectingHandler()),
            Options.Create(new CloverCatalogOptions { BaseUrl = "https://api.clover.com.attacker.test" }),
            TimeProvider.System);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            gateway.GetInventoryAsync("merchant-1", "secret", ["item-1"]));
    }

    private sealed class RejectingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new Xunit.Sdk.XunitException("No request should be sent to a rejected host.");
    }
}

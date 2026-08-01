using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class CloverCatalogGatewayTests
{
    [Fact]
    public async Task GetCatalogAsync_ScopesAllOfficialHostRequestsToMerchant()
    {
        var handler = new SequenceHandler("""
            { "elements": [{ "id": "cat-1", "name": "Lunch" }] }
            """, """
            { "elements": [{ "id": "item-1", "name": "Burger", "price": 1200, "priceType": "FIXED",
              "categories": { "elements": [{ "id": "cat-1" }] } }] }
            """, """
            { "elements": [] }
            """);
        var gateway = new CloverCatalogGateway(new HttpClient(handler), Options.Create(new CloverCatalogOptions()));

        var result = await gateway.GetCatalogAsync("merchant-123", "access-secret");

        Assert.Equal(3, handler.Requests.Count);
        Assert.All(handler.Requests, uri =>
        {
            Assert.Equal("api.clover.com", uri.Host);
            Assert.Contains("/v3/merchants/merchant-123/", uri.AbsolutePath, StringComparison.Ordinal);
        });
        Assert.Equal("Burger", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task GetCatalogAsync_RejectsLookalikeHostBeforeSending()
    {
        var options = Options.Create(new CloverCatalogOptions { BaseUrl = "https://api.clover.com.attacker.test" });
        var gateway = new CloverCatalogGateway(new HttpClient(new SequenceHandler()), options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => gateway.GetCatalogAsync("merchant-123", "secret"));
    }

    [Fact]
    public void Map_TranslatesCategoriesFixedPriceItemsAndAssociatedModifiers()
    {
        using var categories = JsonDocument.Parse("""
            [{ "id": "cat-1", "name": "Lunch", "sortOrder": 4, "deleted": false }]
            """);
        using var items = JsonDocument.Parse("""
            [{
              "id": "item-1", "name": "Burger", "price": 1250, "priceType": "FIXED",
              "categories": { "elements": [{ "id": "cat-1", "name": "Lunch" }] },
              "modifierGroups": { "elements": [{ "id": "group-1", "name": "Add-ons" }] }
            }]
            """);
        using var modifiers = JsonDocument.Parse("""
            [{ "id": "mod-1", "name": "Cheese", "price": 150, "modifierGroup": { "id": "group-1" } }]
            """);

        var result = CloverCatalogGateway.Map(Elements(categories), Elements(items), Elements(modifiers), "USD");

        Assert.Equal("Lunch", Assert.Single(result.Categories).Name);
        var item = Assert.Single(result.Items);
        Assert.Equal("cat-1", item.ExternalCategoryId);
        Assert.Equal(12.50m, item.Price);
        Assert.Equal("USD", item.CurrencyCode);
        var modifier = Assert.Single(item.Modifiers);
        Assert.Equal("Cheese", modifier.Name);
        Assert.Equal(1.50m, modifier.PriceAdjustment);
    }

    [Fact]
    public void Map_AssignsDeterministicUncategorizedAndRejectsVariablePriceAtImportBoundary()
    {
        using var categories = JsonDocument.Parse("[]");
        using var items = JsonDocument.Parse("""
            [{ "id": "item-2", "name": "Market Price", "price": 999, "priceType": "VARIABLE" }]
            """);
        using var modifiers = JsonDocument.Parse("[]");

        var result = CloverCatalogGateway.Map(Elements(categories), Elements(items), Elements(modifiers), "USD");

        Assert.Equal("clover-uncategorized", Assert.Single(result.Categories).ExternalId);
        var item = Assert.Single(result.Items);
        Assert.Equal("clover-uncategorized", item.ExternalCategoryId);
        Assert.Equal(string.Empty, item.CurrencyCode);
    }

    private static JsonElement[] Elements(JsonDocument document) =>
        document.RootElement.EnumerateArray().Select(value => value.Clone()).ToArray();

    private sealed class SequenceHandler(params string[] responses) : HttpMessageHandler
    {
        private int index;
        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            var body = index < responses.Length ? responses[index++] : "{ \"elements\": [] }";
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}

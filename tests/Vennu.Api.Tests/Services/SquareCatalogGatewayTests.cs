using System.Text.Json;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class SquareCatalogGatewayTests
{
    [Fact]
    public void Map_TranslatesCategoryVariationAndReferencedModifiers()
    {
        using var document = JsonDocument.Parse("""
            [
              { "type": "CATEGORY", "id": "cat-1", "category_data": { "name": "Food" } },
              { "type": "MODIFIER_LIST", "id": "list-1", "modifier_list_data": { "modifiers": [
                { "id": "mod-1", "modifier_data": { "name": "Cheese", "price_money": { "amount": 150, "currency": "USD" } } }
              ] } },
              { "type": "ITEM", "id": "item-1", "item_data": {
                "name": "Burger", "description": "Beef",
                "categories": [{ "id": "cat-1" }],
                "modifier_list_info": [{ "modifier_list_id": "list-1" }],
                "variations": [{ "id": "variation-1", "item_variation_data": {
                  "name": "Regular", "price_money": { "amount": 1250, "currency": "USD" }
                } }]
              } }
            ]
            """);

        var result = SquareCatalogGateway.Map(document.RootElement.EnumerateArray().Select(value => value.Clone()).ToArray());

        var category = Assert.Single(result.Categories);
        Assert.Equal("cat-1", category.ExternalId);
        var item = Assert.Single(result.Items);
        Assert.Equal("variation-1", item.ExternalId);
        Assert.Equal("cat-1", item.ExternalCategoryId);
        Assert.Equal("Burger", item.Name);
        Assert.Equal(12.50m, item.Price);
        Assert.Equal("Cheese", Assert.Single(item.Modifiers).Name);
    }

    [Fact]
    public void Map_MissingPriceMoney_RemainsUnsupportedForImportValidation()
    {
        using var document = JsonDocument.Parse("""
            [{ "type": "ITEM", "id": "item-1", "item_data": {
              "name": "Market Price", "category_id": "cat-1",
              "variations": [{ "id": "variation-1", "item_variation_data": { "name": "Regular" } }]
            } }]
            """);

        var result = SquareCatalogGateway.Map(document.RootElement.EnumerateArray().Select(value => value.Clone()).ToArray());

        Assert.Equal(string.Empty, Assert.Single(result.Items).CurrencyCode);
    }
}

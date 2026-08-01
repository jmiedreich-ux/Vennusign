using System.Text.Json;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class ToastCatalogGatewayTests
{
    [Fact]
    public void Map_TranslatesPublishedGroupsAndItems()
    {
        using var document = JsonDocument.Parse("""
            {"menus":[{"guid":"menu-1","name":"Main","menuGroups":[
              {"guid":"group-1","name":"Lunch","menuItems":[
                {"guid":"item-1","name":"Burger","description":"Beef","price":12.50}
              ]}
            ]}]}
            """);

        var result = ToastCatalogGateway.Map(document.RootElement);

        Assert.Equal("group-1", Assert.Single(result.Categories).ExternalId);
        var item = Assert.Single(result.Items);
        Assert.Equal("item-1", item.ExternalId);
        Assert.Equal("group-1", item.ExternalCategoryId);
        Assert.Equal(12.50m, item.Price);
        Assert.Equal("USD", item.CurrencyCode);
    }

    [Fact]
    public void Map_MissingMenus_ReturnsEmptyCatalog()
    {
        using var document = JsonDocument.Parse("{}");
        var result = ToastCatalogGateway.Map(document.RootElement);
        Assert.Empty(result.Categories);
        Assert.Empty(result.Items);
    }
}

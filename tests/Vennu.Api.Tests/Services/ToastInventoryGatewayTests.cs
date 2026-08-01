using System.Text.Json;
using Vennu.Api.Pos;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class ToastInventoryGatewayTests
{
    [Fact]
    public void Map_TranslatesSupportedValidStates()
    {
        using var document = JsonDocument.Parse("""
            [
              {"guid":"11111111-1111-1111-1111-111111111111","itemGuidValidity":"VALID","status":"IN_STOCK","quantity":null},
              {"guid":"22222222-2222-2222-2222-222222222222","itemGuidValidity":"VALID","status":"QUANTITY","quantity":3},
              {"guid":"33333333-3333-3333-3333-333333333333","itemGuidValidity":"VALID","status":"OUT_OF_STOCK","quantity":null},
              {"guid":"44444444-4444-4444-4444-444444444444","itemGuidValidity":"INVALID","status":"OUT_OF_STOCK","quantity":null}
            ]
            """);

        var items = ToastInventoryGateway.Map(document.RootElement).OrderBy(value => value.ExternalItemId).ToArray();

        Assert.Equal(3, items.Length);
        Assert.True(items[0].IsAvailable);
        Assert.Null(items[0].QuantityAvailable);
        Assert.True(items[1].IsAvailable);
        Assert.Equal(3, items[1].QuantityAvailable);
        Assert.False(items[2].IsAvailable);
        Assert.Equal(0, items[2].QuantityAvailable);
    }

    [Fact]
    public void Map_FractionalQuantity_RemainsAvailableWithoutInventingIntegerCount()
    {
        using var document = JsonDocument.Parse("""
            [{"guid":"11111111-1111-1111-1111-111111111111","itemGuidValidity":"VALID","status":"QUANTITY","quantity":1.5}]
            """);

        var item = Assert.Single(ToastInventoryGateway.Map(document.RootElement));

        Assert.True(item.IsAvailable);
        Assert.Null(item.QuantityAvailable);
    }
}

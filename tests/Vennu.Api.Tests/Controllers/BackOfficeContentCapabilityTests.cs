using Vennu.Api.BackOffice;
using Vennu.Api.Controllers.BackOffice;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeContentCapabilityTests
{
    [Fact]
    public void PasteImportRoutes_RequireImportCapability()
    {
        var capability = Assert.Single(typeof(BackOfficeMenuImportsController)
            .GetCustomAttributes(typeof(RequireCapabilityAttribute), inherit: true)
            .Cast<RequireCapabilityAttribute>());

        Assert.Equal("content.menu.import", Assert.Single(capability.Arguments!));
    }

    [Theory]
    [InlineData(nameof(BackOfficeContentController.GetQuickUpdateBoard))]
    [InlineData(nameof(BackOfficeContentController.SetAvailability))]
    [InlineData(nameof(BackOfficeContentController.RestoreAllAvailability))]
    public void QuickUpdateRoutes_RequireOnlyAvailabilityCapability(string action)
    {
        var method = typeof(BackOfficeContentController).GetMethod(action);
        var capability = Assert.Single(method!.GetCustomAttributes(typeof(RequireCapabilityAttribute), inherit: true)
            .Cast<RequireCapabilityAttribute>());

        Assert.Equal("content.item.availability_update", Assert.Single(capability.Arguments!));
    }
}

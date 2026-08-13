using Vennu.Api.BackOffice;
using Vennu.Api.Controllers.BackOffice;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeContentCapabilityTests
{
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

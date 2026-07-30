using Vennu.Api.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class PhotoGridDensityTests
{
    [Theory]
    [InlineData("2x2", 4)]
    [InlineData("3x2", 6)]
    [InlineData("4x2", 8)]
    [InlineData("3x3", 9)]
    public void Capacity_ReturnsStableSupportedCapacity(string density, int expected) =>
        Assert.Equal(expected, PhotoGridDensity.Capacity(density));

    [Fact]
    public void Normalize_UsesDefaultAndRejectsUnknown()
    {
        Assert.Equal("3x2", PhotoGridDensity.Normalize(null));
        Assert.Throws<ArgumentException>(() => PhotoGridDensity.Normalize("5x2"));
    }
}

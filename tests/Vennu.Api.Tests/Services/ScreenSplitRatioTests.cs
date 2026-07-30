using Vennu.Api.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class ScreenSplitRatioTests
{
    [Theory]
    [InlineData(null, "40_60")]
    [InlineData(" 40/60 ", "40_60")]
    [InlineData("50-50", "50_50")]
    public void Normalize_ReturnsSupportedStableValue(string? input, string expected) =>
        Assert.Equal(expected, ScreenSplitRatio.Normalize(input));

    [Fact]
    public void Normalize_RejectsUnsupportedRatio() =>
        Assert.Throws<ArgumentException>(() => ScreenSplitRatio.Normalize("30_70"));
}

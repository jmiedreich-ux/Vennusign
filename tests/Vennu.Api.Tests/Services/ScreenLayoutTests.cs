using Vennu.Api.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class ScreenLayoutTests
{
    [Theory]
    [InlineData("photo_grid", "photo_grid")]
    [InlineData(" Classic-Diner ", "classic_diner")]
    [InlineData(" Neon Chalkboard ", "neon_chalkboard")]
    [InlineData(" Split Layout ", "split_layout")]
    [InlineData(" Daily Special Hero ", "daily_special_hero")]
    [InlineData(" Classic Chalkboard ", "classic_chalkboard")]
    [InlineData(" Tap Strips ", "tap_strips")]
    public void Normalize_ReturnsSupportedStableKey(string input, string expected) =>
        Assert.Equal(expected, ScreenLayout.Normalize(input));

    [Fact]
    public void Normalize_DefaultsAndRejectsUnknown()
    {
        Assert.Equal("photo_grid", ScreenLayout.Normalize(null));
        Assert.Throws<ArgumentException>(() => ScreenLayout.Normalize("neon"));
    }
}

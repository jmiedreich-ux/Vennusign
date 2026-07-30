using Vennu.Core.Models;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class PlaylistSlideTypeTests
{
    [Theory]
    [InlineData("menu", "menu")]
    [InlineData(" IMAGE ", "image")]
    [InlineData("message", "message")]
    public void Normalize_ReturnsSupportedType(string value, string expected) =>
        Assert.Equal(expected, PlaylistSlideType.Normalize(value));

    [Fact]
    public void Normalize_RejectsUnsupportedType() =>
        Assert.Throws<ArgumentException>(() => PlaylistSlideType.Normalize("video"));
}

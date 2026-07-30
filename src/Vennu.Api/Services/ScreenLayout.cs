namespace Vennu.Api.Services;

public static class ScreenLayout
{
    public const string Default = "photo_grid";

    private static readonly HashSet<string> Supported =
        new(StringComparer.OrdinalIgnoreCase) { "photo_grid", "classic_diner", "neon_chalkboard", "split_layout", "daily_special_hero" };

    public static string Normalize(string? layout)
    {
        var normalized = string.IsNullOrWhiteSpace(layout)
            ? Default
            : layout.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');
        return Supported.Contains(normalized)
            ? normalized
            : throw new ArgumentException("Display layout must be photo_grid, classic_diner, neon_chalkboard, split_layout, or daily_special_hero.", nameof(layout));
    }
}

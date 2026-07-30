namespace Vennu.Api.Services;

public static class ScreenLayout
{
    public const string Default = "photo_grid";

    private static readonly HashSet<string> Supported =
        new(StringComparer.OrdinalIgnoreCase) { "photo_grid", "classic_diner" };

    public static string Normalize(string? layout)
    {
        var normalized = string.IsNullOrWhiteSpace(layout)
            ? Default
            : layout.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');
        return Supported.Contains(normalized)
            ? normalized
            : throw new ArgumentException("Display layout must be photo_grid or classic_diner.", nameof(layout));
    }
}

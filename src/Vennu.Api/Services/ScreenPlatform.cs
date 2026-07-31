namespace Vennu.Api.Services;

public static class ScreenPlatform
{
    private static readonly string[] Supported = ["android_tv", "fire_tv", "tizen", "webos"];

    public static string Normalize(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return Supported.Contains(normalized, StringComparer.Ordinal)
            ? normalized!
            : throw new ArgumentException("Platform must be android_tv, fire_tv, tizen, or webos.", nameof(value));
    }

    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant() is "browser" or "web"
                ? "browser"
                : Normalize(value);
}

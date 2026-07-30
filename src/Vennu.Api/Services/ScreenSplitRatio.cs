namespace Vennu.Api.Services;

public static class ScreenSplitRatio
{
    public const string Default = "40_60";

    public static string Normalize(string? ratio)
    {
        var normalized = string.IsNullOrWhiteSpace(ratio)
            ? Default
            : ratio.Trim().Replace('/', '_').Replace('-', '_');
        return normalized is "40_60" or "50_50"
            ? normalized
            : throw new ArgumentException("Split ratio must be 40_60 or 50_50.", nameof(ratio));
    }
}

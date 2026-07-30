namespace Vennu.Api.Services;

public static class PhotoGridDensity
{
    public const string Default = "3x2";

    private static readonly IReadOnlyDictionary<string, int> Capacities =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["2x2"] = 4,
            ["3x2"] = 6,
            ["4x2"] = 8,
            ["3x3"] = 9
        };

    public static string Normalize(string? density)
    {
        var normalized = string.IsNullOrWhiteSpace(density) ? Default : density.Trim().ToLowerInvariant();
        return Capacities.ContainsKey(normalized)
            ? normalized
            : throw new ArgumentException("Photo Grid density must be one of 2x2, 3x2, 4x2, or 3x3.", nameof(density));
    }

    public static int Capacity(string? density) => Capacities[Normalize(density)];
}

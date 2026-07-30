using Vennu.Api.Contracts.Admin;

namespace Vennu.Api.Services;

public static class VenueThemePresets
{
    private static readonly IReadOnlyDictionary<string, VenueThemePresetResponse> Presets =
        new[]
        {
            Create("bar_classic", "Bar Classic", "#F8F5E9", "#00E5FF", "#071013", ["#00E5FF", "#FF2BD6", "#FFE66D", "#7CFF6B"], 1.00m, "Righteous", "Caveat"),
            Create("violet_lounge", "Violet Lounge", "#F5E9FF", "#A855F7", "#12091C", ["#C084FC", "#F472B6", "#818CF8", "#E879F9"], 1.15m, "Pacifico", "Kalam"),
            Create("hot_summer", "Hot Summer", "#FFF4D6", "#FF5A36", "#1B0B08", ["#FF5A36", "#FFB000", "#FF2D95", "#FFF06A"], 1.30m, "Bungee", "Permanent Marker"),
            Create("ocean_dive", "Ocean Dive", "#E8FCFF", "#00C2FF", "#06141D", ["#00C2FF", "#2DE2E6", "#4D8CFF", "#71F79F"], 1.10m, "Fredoka One", "Patrick Hand"),
            Create("rose_gold", "Rose Gold", "#FFF1F5", "#E8A0B5", "#170D12", ["#E8A0B5", "#F6C1C7", "#D8A7B1", "#FFD1DC"], 0.85m, "Lobster", "Kalam")
        }.ToDictionary(preset => preset.Key, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<VenueThemePresetResponse> GetAll() =>
        Presets.Values.OrderBy(preset => preset.Label, StringComparer.Ordinal).ToArray();

    public static VenueThemePresetResponse Get(string key)
    {
        var normalized = key?.Trim();
        return normalized is not null && Presets.TryGetValue(normalized, out var preset)
            ? preset
            : throw new KeyNotFoundException("Advanced theme preset does not exist.");
    }

    private static VenueThemePresetResponse Create(
        string key,
        string label,
        string titleColor,
        string glowColor,
        string boardBackgroundColor,
        IReadOnlyCollection<string> sectionColors,
        decimal glowIntensity,
        string titleFont,
        string itemFont) =>
        new(key, label, titleColor, glowColor, boardBackgroundColor, sectionColors, glowIntensity, titleFont, itemFont);
}

namespace Vennu.Api.Contracts.PlatformOperations;

public sealed record VenueThemeResponse(
    Guid VenueId,
    string BackgroundColor,
    string AccentColor,
    string FontFamily,
    string PresetKey,
    string TitleColor,
    string GlowColor,
    string BoardBackgroundColor,
    IReadOnlyCollection<string> SectionColors,
    decimal GlowIntensity,
    string TitleFont,
    string ItemFont,
    DateTime UpdatedUtc);

public sealed record VenueThemeUpdateRequest(
    string BackgroundColor,
    string AccentColor,
    string FontFamily);

public sealed record VenueAdvancedThemeUpdateRequest(
    string TitleColor,
    string GlowColor,
    string BoardBackgroundColor,
    IReadOnlyCollection<string> SectionColors,
    decimal GlowIntensity,
    string TitleFont,
    string ItemFont);

public sealed record VenueThemePresetResponse(
    string Key,
    string Label,
    string TitleColor,
    string GlowColor,
    string BoardBackgroundColor,
    IReadOnlyCollection<string> SectionColors,
    decimal GlowIntensity,
    string TitleFont,
    string ItemFont);

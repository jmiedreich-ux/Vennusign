using System.Text.RegularExpressions;
using Vennu.Api.Contracts.Admin;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Api.Notifications;

namespace Vennu.Api.Services;

public sealed partial class VenueThemeService(
    IVenueRepository venueRepository,
    IVenueThemeRepository themeRepository,
    TimeProvider timeProvider,
    IScreenUpdateNotifier? notifier = null) : IVenueThemeService
{
    private static readonly IReadOnlyDictionary<string, string> ApprovedFonts =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Inter"] = "Inter",
            ["Georgia"] = "Georgia",
            ["Arial"] = "Arial"
        };
    private static readonly IReadOnlyDictionary<string, string> ApprovedTitleFonts =
        new[] { "Pacifico", "Lobster", "Righteous", "Fredoka One", "Bungee", "Permanent Marker" }
            .ToDictionary(font => font, StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyDictionary<string, string> ApprovedItemFonts =
        new[] { "Caveat", "Kalam", "Patrick Hand", "Permanent Marker" }
            .ToDictionary(font => font, StringComparer.OrdinalIgnoreCase);

    public async Task<VenueThemeResponse> GetAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var theme = await themeRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? new VenueTheme { VenueId = venueId };
        return ToResponse(theme);
    }

    public async Task<VenueThemeResponse> UpdateAsync(
        Guid venueId,
        string backgroundColor,
        string accentColor,
        string fontFamily,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var theme = await GetOrCreateThemeAsync(venueId, cancellationToken).ConfigureAwait(false);
        theme.BackgroundColor = NormalizeColor(backgroundColor, nameof(backgroundColor));
        theme.AccentColor = NormalizeColor(accentColor, nameof(accentColor));
        theme.FontFamily = NormalizeFont(fontFamily);
        return await SaveAsync(theme, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VenueThemeResponse> UpdateAdvancedAsync(
        Guid venueId,
        VenueAdvancedThemeUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var theme = await GetOrCreateThemeAsync(venueId, cancellationToken).ConfigureAwait(false);
        ApplyAdvanced(
            theme,
            "custom",
            request.TitleColor,
            request.GlowColor,
            request.BoardBackgroundColor,
            request.SectionColors,
            request.GlowIntensity,
            request.TitleFont,
            request.ItemFont);
        return await SaveAsync(theme, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VenueThemeResponse> ApplyPresetAsync(
        Guid venueId,
        string presetKey,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var preset = VenueThemePresets.Get(presetKey);
        var theme = await GetOrCreateThemeAsync(venueId, cancellationToken).ConfigureAwait(false);
        ApplyAdvanced(
            theme,
            preset.Key,
            preset.TitleColor,
            preset.GlowColor,
            preset.BoardBackgroundColor,
            preset.SectionColors,
            preset.GlowIntensity,
            preset.TitleFont,
            preset.ItemFont);
        return await SaveAsync(theme, cancellationToken).ConfigureAwait(false);
    }

    private async Task RequireVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        if (await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Venue does not exist.");
        }
    }

    private static string NormalizeColor(string color, string parameterName)
    {
        var normalized = color?.Trim().ToUpperInvariant();
        return normalized is not null && HexColor().IsMatch(normalized)
            ? normalized
            : throw new ArgumentException("Theme colors must use #RRGGBB format.", parameterName);
    }

    private static string NormalizeFont(string fontFamily)
    {
        var normalized = fontFamily?.Trim();
        return normalized is not null && ApprovedFonts.TryGetValue(normalized, out var approved)
            ? approved
            : throw new ArgumentException("Font family must be Inter, Georgia, or Arial.", nameof(fontFamily));
    }

    private async Task<VenueTheme> GetOrCreateThemeAsync(Guid venueId, CancellationToken cancellationToken) =>
        await themeRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? new VenueTheme { VenueId = venueId };

    private async Task<VenueThemeResponse> SaveAsync(VenueTheme theme, CancellationToken cancellationToken)
    {
        theme.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await themeRepository.UpsertAsync(theme, cancellationToken).ConfigureAwait(false);
        var response = ToResponse(theme);
        if (notifier is not null)
        {
            await notifier.NotifyVenueThemeUpdatedAsync(theme.VenueId, response, cancellationToken).ConfigureAwait(false);
        }
        return response;
    }

    private static void ApplyAdvanced(
        VenueTheme theme,
        string presetKey,
        string titleColor,
        string glowColor,
        string boardBackgroundColor,
        IReadOnlyCollection<string> sectionColors,
        decimal glowIntensity,
        string titleFont,
        string itemFont)
    {
        ArgumentNullException.ThrowIfNull(sectionColors);
        if (sectionColors.Count is < 1 or > 4)
        {
            throw new ArgumentException("Advanced themes require between one and four section colors.", nameof(sectionColors));
        }
        if (glowIntensity is < 0.20m or > 2.00m)
        {
            throw new ArgumentOutOfRangeException(nameof(glowIntensity), "Glow intensity must be between 0.20 and 2.00.");
        }

        theme.PresetKey = presetKey;
        theme.TitleColor = NormalizeColor(titleColor, nameof(titleColor));
        theme.GlowColor = NormalizeColor(glowColor, nameof(glowColor));
        theme.BoardBackgroundColor = NormalizeColor(boardBackgroundColor, nameof(boardBackgroundColor));
        theme.SectionColors = string.Join(',', sectionColors.Select((color, index) =>
            NormalizeColor(color, $"{nameof(sectionColors)}[{index}]")));
        theme.GlowIntensity = glowIntensity;
        theme.TitleFont = NormalizeAdvancedFont(titleFont, ApprovedTitleFonts, nameof(titleFont));
        theme.ItemFont = NormalizeAdvancedFont(itemFont, ApprovedItemFonts, nameof(itemFont));
    }

    private static string NormalizeAdvancedFont(
        string font,
        IReadOnlyDictionary<string, string> approvedFonts,
        string parameterName)
    {
        var normalized = font?.Trim();
        return normalized is not null && approvedFonts.TryGetValue(normalized, out var approved)
            ? approved
            : throw new ArgumentException("Advanced theme font is not approved.", parameterName);
    }

    private static VenueThemeResponse ToResponse(VenueTheme theme) =>
        new(
            theme.VenueId,
            theme.BackgroundColor,
            theme.AccentColor,
            theme.FontFamily,
            theme.PresetKey,
            theme.TitleColor,
            theme.GlowColor,
            theme.BoardBackgroundColor,
            theme.SectionColors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            theme.GlowIntensity,
            theme.TitleFont,
            theme.ItemFont,
            theme.UpdatedUtc);

    [GeneratedRegex("^#[0-9A-F]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexColor();
}

using System.Text.RegularExpressions;
using Vennu.Api.Contracts.Admin;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Services;

public sealed partial class VenueThemeService(
    IVenueRepository venueRepository,
    IVenueThemeRepository themeRepository,
    TimeProvider timeProvider) : IVenueThemeService
{
    private static readonly IReadOnlyDictionary<string, string> ApprovedFonts =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Inter"] = "Inter",
            ["Georgia"] = "Georgia",
            ["Arial"] = "Arial"
        };

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
        var theme = new VenueTheme
        {
            VenueId = venueId,
            BackgroundColor = NormalizeColor(backgroundColor, nameof(backgroundColor)),
            AccentColor = NormalizeColor(accentColor, nameof(accentColor)),
            FontFamily = NormalizeFont(fontFamily),
            UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime
        };
        await themeRepository.UpsertAsync(theme, cancellationToken).ConfigureAwait(false);
        return ToResponse(theme);
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

    private static VenueThemeResponse ToResponse(VenueTheme theme) =>
        new(theme.VenueId, theme.BackgroundColor, theme.AccentColor, theme.FontFamily, theme.UpdatedUtc);

    [GeneratedRegex("^#[0-9A-F]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexColor();
}

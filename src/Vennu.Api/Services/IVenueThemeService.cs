using Vennu.Api.Contracts.Admin;

namespace Vennu.Api.Services;

public interface IVenueThemeService
{
    Task<VenueThemeResponse> GetAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<VenueThemeResponse> UpdateAsync(
        Guid venueId,
        string backgroundColor,
        string accentColor,
        string fontFamily,
        CancellationToken cancellationToken = default);

    Task<VenueThemeResponse> UpdateAdvancedAsync(
        Guid venueId,
        VenueAdvancedThemeUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task<VenueThemeResponse> ApplyPresetAsync(
        Guid venueId,
        string presetKey,
        CancellationToken cancellationToken = default);
}

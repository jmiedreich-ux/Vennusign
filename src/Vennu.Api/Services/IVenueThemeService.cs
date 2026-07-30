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
}

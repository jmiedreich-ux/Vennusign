using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Api.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues/{venueId:guid}/theme")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminThemesController(IVenueThemeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<VenueThemeResponse>> Get(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.GetAsync(venueId, cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut]
    public async Task<ActionResult<VenueThemeResponse>> Update(
        Guid venueId,
        VenueThemeUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.UpdateAsync(
                venueId,
                request.BackgroundColor,
                request.AccentColor,
                request.FontFamily,
                cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }
}

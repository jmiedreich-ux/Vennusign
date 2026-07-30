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
    [HttpGet("presets")]
    public ActionResult<IReadOnlyCollection<VenueThemePresetResponse>> GetPresets() =>
        Ok(VenueThemePresets.GetAll());

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

    [HttpPut("advanced")]
    public async Task<ActionResult<VenueThemeResponse>> UpdateAdvanced(
        Guid venueId,
        VenueAdvancedThemeUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.UpdateAdvancedAsync(venueId, request, cancellationToken).ConfigureAwait(false));
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

    [HttpPut("presets/{presetKey}")]
    public async Task<ActionResult<VenueThemeResponse>> ApplyPreset(
        Guid venueId,
        string presetKey,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ApplyPresetAsync(venueId, presetKey, cancellationToken).ConfigureAwait(false));
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/venues/{venueId:guid}/screens/{screenId:guid}/playlist")]
[Route("api/venue-admin/venues/{venueId:guid}/screens/{screenId:guid}/playlist")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[BackOfficeVenueScope]
public sealed class BackOfficePlaylistsController(IPlaylistAdministrationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PlaylistSlide>>> Get(
        Guid venueId, Guid screenId, CancellationToken cancellationToken)
    {
        try { return Ok(await service.GetAsync(venueId, screenId, cancellationToken).ConfigureAwait(false)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistSlide>> Create(
        Guid venueId, Guid screenId, PlaylistSlideWriteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(venueId, screenId, request.ToWrite(), cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId, screenId }, created);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPut("{slideId:guid}")]
    public async Task<ActionResult<PlaylistSlide>> Update(
        Guid venueId, Guid screenId, Guid slideId, PlaylistSlideWriteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.UpdateAsync(venueId, screenId, slideId, request.ToWrite(), cancellationToken).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPut("order")]
    public async Task<ActionResult<IReadOnlyCollection<PlaylistSlide>>> Reorder(
        Guid venueId, Guid screenId, PlaylistReorderRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await service.ReorderAsync(venueId, screenId, request.OrderedIds, cancellationToken).ConfigureAwait(false)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpDelete("{slideId:guid}")]
    public async Task<IActionResult> Delete(
        Guid venueId, Guid screenId, Guid slideId, CancellationToken cancellationToken)
    {
        try { return await service.DeleteAsync(venueId, screenId, slideId, cancellationToken).ConfigureAwait(false) ? NoContent() : NotFound(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }
}

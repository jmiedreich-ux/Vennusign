using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues/{venueId:guid}/menus")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminMenusController(IMenuSectionManagementService service) : ControllerBase
{
    [HttpGet]
    public Task<MenuEditorSnapshot> Get(Guid venueId, CancellationToken cancellationToken) =>
        service.GetAsync(venueId, cancellationToken);

    [HttpPost("{menuId:guid}/sections")]
    public async Task<ActionResult<MenuSection>> CreateSection(
        Guid venueId,
        Guid menuId,
        MenuSectionCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var section = await service.CreateAsync(venueId, menuId, request.Name, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId }, section);
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

    [HttpPut("sections/{sectionId:guid}")]
    public async Task<ActionResult<MenuSection>> UpdateSection(
        Guid venueId,
        Guid sectionId,
        MenuSectionUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var section = await service.UpdateAsync(venueId, sectionId, request.Name, request.IsActive, cancellationToken).ConfigureAwait(false);
            return section is null ? NotFound() : Ok(section);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{menuId:guid}/sections/order")]
    public async Task<ActionResult> Reorder(
        Guid venueId,
        Guid menuId,
        MenuSectionOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.SectionIds is null)
            {
                return ValidationProblem("Section identifiers are required.");
            }
            await service.ReorderAsync(venueId, menuId, request.SectionIds, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }
}

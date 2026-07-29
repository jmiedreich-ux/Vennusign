using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Core.Models;
using Vennu.Data.Services;
using Vennu.Api.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues/{venueId:guid}/menus")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminMenusController(
    IMenuSectionManagementService sectionService,
    IMenuItemManagementService itemService) : ControllerBase
{
    [HttpGet]
    public Task<MenuEditorSnapshot> Get(Guid venueId, CancellationToken cancellationToken) =>
        sectionService.GetAsync(venueId, cancellationToken);

    [HttpPost("{menuId:guid}/sections")]
    public async Task<ActionResult<MenuSection>> CreateSection(
        Guid venueId,
        Guid menuId,
        MenuSectionCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var section = await sectionService.CreateAsync(venueId, menuId, request.Name, cancellationToken).ConfigureAwait(false);
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
            var section = await sectionService.UpdateAsync(venueId, sectionId, request.Name, request.IsActive, cancellationToken).ConfigureAwait(false);
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
            await sectionService.ReorderAsync(venueId, menuId, request.SectionIds, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPost("{menuId:guid}/sections/{sectionId:guid}/items")]
    public async Task<ActionResult<MenuItem>> CreateItem(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        MenuItemWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await itemService.CreateAsync(
                venueId,
                menuId,
                sectionId,
                request.Name,
                request.Description,
                request.Price,
                request.HappyHourPrice,
                cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId }, item);
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

    [HttpPut("{menuId:guid}/sections/{sectionId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<MenuItem>> UpdateItem(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        MenuItemWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await itemService.UpdateAsync(
                venueId,
                menuId,
                sectionId,
                itemId,
                request.Name,
                request.Description,
                request.Price,
                request.HappyHourPrice,
                cancellationToken).ConfigureAwait(false);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }
}

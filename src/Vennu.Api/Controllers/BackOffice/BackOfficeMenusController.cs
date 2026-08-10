using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Services;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

/// <summary>
/// The current editor's surface, consolidated onto the item library: every write
/// here changes the working state that the derived draft compares and a publish
/// ships. The owner-killed concepts (daily special, quantities, tags, popular,
/// archive) have no endpoints any more — there is no legacy, because it was
/// never live.
/// </summary>
[ApiController]
[Route("api/back-office/menus")]
[Route("api/venue-admin/menus")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[RequireCapability("content.item.update")]
public sealed class BackOfficeMenusController(
    IMenuSectionManagementService sectionService,
    IMenuItemManagementService itemService,
    ContentService content) : ControllerBase
{
    private Guid VenueId => Guid.Parse(
        User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);

    private string? Author =>
        User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email);

    [HttpGet]
    public Task<MenuEditorSnapshot> Get(CancellationToken cancellationToken) =>
        sectionService.GetAsync(VenueId, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<Menu>> CreateMenu(
        MenuCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var menu = await sectionService.CreateMenuAsync(VenueId, request.Name, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), menu);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPost("{menuId:guid}/sections")]
    public async Task<ActionResult<MenuSection>> CreateSection(
        Guid menuId,
        MenuSectionCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var section = await sectionService
                .CreateAsync(VenueId, menuId, request.Name, cancellationToken)
                .ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), section);
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
        Guid sectionId,
        MenuSectionUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var section = await sectionService
                .UpdateAsync(VenueId, sectionId, request.Name, cancellationToken)
                .ConfigureAwait(false);
            return section is null ? NotFound() : Ok(section);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{menuId:guid}/sections/order")]
    public async Task<ActionResult> Reorder(
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

            await sectionService
                .ReorderAsync(VenueId, menuId, request.SectionIds, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPost("{menuId:guid}/sections/{sectionId:guid}/items")]
    public async Task<ActionResult<MenuItem>> CreateItem(
        Guid menuId,
        Guid sectionId,
        MenuItemWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await itemService
                .CreateAsync(
                    VenueId,
                    menuId,
                    sectionId,
                    request.Name,
                    request.Description,
                    request.Price,
                    cancellationToken)
                .ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), item);
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
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        MenuItemWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await itemService
                .UpdateAsync(
                    VenueId,
                    menuId,
                    sectionId,
                    itemId,
                    request.Name,
                    request.Description,
                    request.Price,
                    cancellationToken)
                .ConfigureAwait(false);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{menuId:guid}/sections/{sectionId:guid}/items/order")]
    public async Task<ActionResult> ReorderItems(
        Guid menuId,
        Guid sectionId,
        MenuItemOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.ItemIds is null)
            {
                return ValidationProblem("Item identifiers are required.");
            }
            await itemService.ReorderAsync(
                VenueId, menuId, sectionId, request.ItemIds, cancellationToken).ConfigureAwait(false);
            return NoContent();
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

    /// <summary>
    /// The legacy quick-update route, kept for the current UI but consolidated
    /// onto the one availability model: an 86 is item-by-venue, instant, never
    /// queued, and survives a publish. The menu and section in the route only
    /// locate the row the caller clicked.
    /// </summary>
    [HttpPut("{menuId:guid}/sections/{sectionId:guid}/items/{itemId:guid}/quick-availability")]
    [RequireCapability("content.item.availability_update")]
    public async Task<ActionResult> UpdateQuickAvailability(
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        QuickAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            await content
                .SetAvailabilityAsync(VenueId, itemId, request.IsAvailable, Author, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}

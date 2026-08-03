using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Services;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/menus")]
[Route("api/venue-admin/menus")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
public sealed class BackOfficeMenusController(
    IMenuSectionManagementService sectionService,
    IMenuItemManagementService itemService,
    IQuickUpdateService quickUpdateService) : ControllerBase
{
    private Guid VenueId => Guid.Parse(
        User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);

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

    [HttpPut("{menuId:guid}/quick-update/daily-special")]
    public async Task<ActionResult<Menu>> UpdateDailySpecial(
        Guid menuId,
        QuickDailySpecialRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var menu = await quickUpdateService
                .UpdateDailySpecialAsync(VenueId, menuId, request.DailySpecial, cancellationToken)
                .ConfigureAwait(false);
            return menu is null ? NotFound() : Ok(menu);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{menuId:guid}/sections/{sectionId:guid}/items/{itemId:guid}/quick-availability")]
    public async Task<ActionResult<MenuItem>> UpdateQuickAvailability(
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        QuickAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await quickUpdateService
                .SetAvailabilityAsync(
                    VenueId,
                    menuId,
                    sectionId,
                    itemId,
                    request.IsAvailable,
                    cancellationToken)
                .ConfigureAwait(false);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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
                .UpdateAsync(VenueId, sectionId, request.Name, request.IsActive, cancellationToken)
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
                    request.HappyHourPrice,
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
                    request.HappyHourPrice,
                    cancellationToken)
                .ConfigureAwait(false);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{menuId:guid}/sections/{sectionId:guid}/items/{itemId:guid}/presentation")]
    public async Task<ActionResult<MenuItem>> UpdateItemPresentation(
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        MenuItemPresentationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await itemService
                .UpdatePresentationAsync(
                    VenueId,
                    menuId,
                    sectionId,
                    itemId,
                    request.IsAvailable,
                    request.QuantityAvailable,
                    request.Tags,
                    request.IsPopular,
                    cancellationToken)
                .ConfigureAwait(false);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{menuId:guid}/sections/{sectionId:guid}/items/{itemId:guid}/lifecycle")]
    public async Task<ActionResult<MenuItem>> UpdateItemLifecycle(
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        MenuItemLifecycleRequest request,
        CancellationToken cancellationToken)
    {
        var item = await itemService.SetActiveAsync(
            VenueId, menuId, sectionId, itemId, request.IsActive, cancellationToken).ConfigureAwait(false);
        return item is null ? NotFound() : Ok(item);
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
}

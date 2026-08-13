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

    [HttpPut("{menuId:guid}")]
    public async Task<ActionResult<Menu>> RenameMenu(Guid menuId, MenuCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await sectionService.RenameMenuAsync(VenueId, menuId, request.Name, cancellationToken).ConfigureAwait(false);
            return menu is null ? NotFound() : Ok(menu);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    /*
     * The section and item writes retired with the editor they served (milestone
     * 3). The builder writes through api/back-office/content, where each rule is
     * decided inside the statement that writes it rather than by a read, a check
     * in C#, and a write.
     *
     * What stays: the editor snapshot, which Home and the locked-section preview
     * still read; creating a menu, which Add-a-menu still uses until milestone 6
     * owns the import routes (Q100); and quick availability, which Home uses.
     */

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

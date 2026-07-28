using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminVenuesController : ControllerBase
{
    private readonly IVenueDirectoryService venueDirectoryService;
    private readonly IVenueSupportDetailService venueSupportDetailService;
    private readonly IVenueFeatureOverrideManagementService overrideManagementService;

    public SuperAdminVenuesController(
        IVenueDirectoryService venueDirectoryService,
        IVenueSupportDetailService venueSupportDetailService,
        IVenueFeatureOverrideManagementService overrideManagementService)
    {
        this.venueDirectoryService = venueDirectoryService;
        this.venueSupportDetailService = venueSupportDetailService;
        this.overrideManagementService = overrideManagementService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<VenueDirectoryItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<VenueDirectoryItem>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? tier,
        [FromQuery] string? status,
        [FromQuery] string? health,
        CancellationToken cancellationToken)
    {
        var venues = await venueDirectoryService
            .SearchAsync(new VenueDirectoryQuery(search, tier, status, health), cancellationToken)
            .ConfigureAwait(false);
        return Ok(venues);
    }

    [HttpGet("{venueId:guid}")]
    [ProducesResponseType<VenueSupportDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VenueSupportDetail>> GetById(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        var detail = await venueSupportDetailService.GetAsync(venueId, cancellationToken).ConfigureAwait(false);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPut("{venueId:guid}/overrides/{featureId:guid}")]
    public async Task<ActionResult<VenueFeatureOverride>> SetOverride(
        Guid venueId,
        Guid featureId,
        VenueFeatureOverrideUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var featureOverride = await overrideManagementService.SetAsync(
                venueId,
                featureId,
                new VenueFeatureOverrideRequest(request.Enabled, request.Reason, request.ExpiresAt),
                cancellationToken).ConfigureAwait(false);
            return featureOverride is null ? NotFound() : Ok(featureOverride);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpDelete("{venueId:guid}/overrides/{featureId:guid}")]
    public async Task<IActionResult> RemoveOverride(
        Guid venueId,
        Guid featureId,
        CancellationToken cancellationToken)
    {
        var removed = await overrideManagementService.RemoveAsync(venueId, featureId, cancellationToken).ConfigureAwait(false);
        return removed is null ? NotFound() : NoContent();
    }
}

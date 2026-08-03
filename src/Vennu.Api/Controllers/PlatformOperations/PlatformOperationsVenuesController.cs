using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Contracts.Venues;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/venues")]
[Route("api/admin/venues")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsVenuesController : ControllerBase
{
    private readonly IVenueDirectoryService venueDirectoryService;
    private readonly IVenueSupportDetailService venueSupportDetailService;
    private readonly IVenueFeatureOverrideManagementService overrideManagementService;
    private readonly IVenueTierSwitchService tierSwitchService;
    private readonly IVenueProvisioningService venueProvisioningService;

    public PlatformOperationsVenuesController(
        IVenueDirectoryService venueDirectoryService,
        IVenueSupportDetailService venueSupportDetailService,
        IVenueFeatureOverrideManagementService overrideManagementService,
        IVenueTierSwitchService tierSwitchService,
        IVenueProvisioningService venueProvisioningService)
    {
        this.venueDirectoryService = venueDirectoryService;
        this.venueSupportDetailService = venueSupportDetailService;
        this.overrideManagementService = overrideManagementService;
        this.tierSwitchService = tierSwitchService;
        this.venueProvisioningService = venueProvisioningService;
    }

    [HttpPost]
    [ProducesResponseType<CreateVenueResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateVenueResponse>> Create(
        [FromBody] CreateVenueRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await venueProvisioningService.ProvisionAsync(
                new Venue
                {
                    Name = request.Name,
                    Timezone = request.Timezone,
                    Type = request.Type,
                    PrimaryLanguage = request.PrimaryLanguage,
                    SecondaryLanguage = request.SecondaryLanguage
                },
                cancellationToken).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { venueId = result.VenueId },
                new CreateVenueResponse { VenueId = result.VenueId });
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Venue provisioning failed",
                Detail = exception.Message
            });
        }
    }

    [HttpPut("{venueId:guid}/tier")]
    public async Task<ActionResult<VenueSubscription>> SwitchTier(
        Guid venueId,
        VenueTierSwitchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tierSwitchService
                .SwitchAsync(venueId, request.TargetTierId, cancellationToken)
                .ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Tier switch failed", Detail = exception.Message });
        }
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

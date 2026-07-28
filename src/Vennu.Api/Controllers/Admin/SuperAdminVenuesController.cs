using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminVenuesController : ControllerBase
{
    private readonly IVenueDirectoryService venueDirectoryService;
    private readonly IVenueSupportDetailService venueSupportDetailService;

    public SuperAdminVenuesController(
        IVenueDirectoryService venueDirectoryService,
        IVenueSupportDetailService venueSupportDetailService)
    {
        this.venueDirectoryService = venueDirectoryService;
        this.venueSupportDetailService = venueSupportDetailService;
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
}

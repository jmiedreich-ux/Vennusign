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

    public SuperAdminVenuesController(IVenueDirectoryService venueDirectoryService) =>
        this.venueDirectoryService = venueDirectoryService;

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
}


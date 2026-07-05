using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.Venues;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly IVenueRepository venueRepository;

    public VenuesController(IVenueRepository venueRepository) => this.venueRepository = venueRepository;

    [HttpPost]
    [ProducesResponseType<CreateVenueResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateVenueResponse>> CreateVenue([FromBody] CreateVenueRequest request, CancellationToken cancellationToken)
    {
        var venue = new Venue
        {
            Name = request.Name.Trim(),
            Timezone = request.Timezone.Trim(),
            Type = request.Type.Trim(),
            PrimaryLanguage = request.PrimaryLanguage.Trim(),
            SecondaryLanguage = string.IsNullOrWhiteSpace(request.SecondaryLanguage) ? null : request.SecondaryLanguage.Trim()
        };

        var venueId = await venueRepository.CreateAsync(venue, cancellationToken);
        var response = new CreateVenueResponse { VenueId = venueId };
        return Created($"/api/venues/{venueId}", response);
    }
}

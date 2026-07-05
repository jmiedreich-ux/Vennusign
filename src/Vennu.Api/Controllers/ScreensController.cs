using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Infrastructure;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/screens")]
public class ScreensController : ControllerBase
{
    private readonly IScreenRepository screenRepository;
    private readonly IScreenPairingCodeRepository screenPairingCodeRepository;
    private readonly IVenueRepository venueRepository;

    public ScreensController(IScreenRepository screenRepository, IScreenPairingCodeRepository screenPairingCodeRepository, IVenueRepository venueRepository) => (this.screenRepository, this.screenPairingCodeRepository, this.venueRepository) = (screenRepository, screenPairingCodeRepository, venueRepository);

    [HttpPost]
    [ProducesResponseType<RegisterScreenResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterScreenResponse>> RegisterScreen([FromBody] RegisterScreenRequest request, CancellationToken cancellationToken)
    {
        var screen = new Screen
        {
            ScreenKey = await GenerateUniqueScreenKeyAsync(cancellationToken),
            Name = request.Name.Trim(),
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            Platform = string.IsNullOrWhiteSpace(request.Platform) ? null : request.Platform.Trim(),
            AppVersion = string.IsNullOrWhiteSpace(request.AppVersion) ? null : request.AppVersion.Trim(),
            Status = "Offline"
        };

        var screenId = await screenRepository.CreateAsync(screen, cancellationToken);
        var response = new RegisterScreenResponse { ScreenId = screenId, ScreenKey = screen.ScreenKey };
        return Created($"/api/screens/{screenId}", response);
    }

    [HttpPost("pairing-code")]
    [ProducesResponseType<CreateScreenPairingCodeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateScreenPairingCodeResponse>> CreatePairingCode([FromBody] CreateScreenPairingCodeRequest request, CancellationToken cancellationToken)
    {
        if (request.ScreenId == Guid.Empty)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> { [nameof(request.ScreenId)] = ["ScreenId is required."] }));
        }

        var screen = await screenRepository.GetByIdAsync(request.ScreenId, cancellationToken);

        if (screen is null)
        {
            return NotFound(new ProblemDetails { Title = "Screen not found.", Detail = $"Screen '{request.ScreenId}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        var pairingCode = new ScreenPairingCode
        {
            Code = await GenerateUniquePairingCodeAsync(cancellationToken),
            ScreenId = request.ScreenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsClaimed = false
        };

        await screenPairingCodeRepository.CreateAsync(pairingCode, cancellationToken);

        var response = new CreateScreenPairingCodeResponse
        {
            Code = pairingCode.Code,
            ScreenId = pairingCode.ScreenId,
            ExpiresAt = pairingCode.ExpiresAt
        };

        return Created($"/api/screens/pairing/{pairingCode.Code}/status", response);
    }

    [HttpGet("pairing/{code}/status")]
    [ProducesResponseType<ScreenPairingStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<ActionResult<ScreenPairingStatusResponse>> GetPairingStatus(string code, CancellationToken cancellationToken)
    {
        var pairingCode = await screenPairingCodeRepository.GetByCodeAsync(code, cancellationToken);

        if (pairingCode is null)
        {
            return NotFound(new ProblemDetails { Title = "Pairing code not found.", Detail = $"Pairing code '{code}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        if (!pairingCode.IsClaimed && pairingCode.ExpiresAt <= DateTime.UtcNow)
        {
            return StatusCode(StatusCodes.Status410Gone, new ProblemDetails { Title = "Pairing code expired.", Detail = $"Pairing code '{code}' has expired.", Status = StatusCodes.Status410Gone });
        }

        var response = new ScreenPairingStatusResponse
        {
            Linked = pairingCode.IsClaimed,
            ScreenId = pairingCode.IsClaimed ? pairingCode.ScreenId : null
        };

        return Ok(response);
    }

    [HttpPost("pairing/{code}/claim")]
    [ProducesResponseType<ClaimScreenPairingCodeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<ActionResult<ClaimScreenPairingCodeResponse>> ClaimPairingCode(string code, [FromBody] ClaimScreenPairingCodeRequest request, CancellationToken cancellationToken)
    {
        if (request.VenueId == Guid.Empty)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> { [nameof(request.VenueId)] = ["VenueId is required."] }));
        }

        var pairingCode = await screenPairingCodeRepository.GetByCodeAsync(code, cancellationToken);

        if (pairingCode is null)
        {
            return NotFound(new ProblemDetails { Title = "Pairing code not found.", Detail = $"Pairing code '{code}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        if (pairingCode.IsClaimed)
        {
            return Conflict(new ProblemDetails { Title = "Pairing code already claimed.", Detail = $"Pairing code '{code}' has already been claimed.", Status = StatusCodes.Status409Conflict });
        }

        if (pairingCode.ExpiresAt <= DateTime.UtcNow)
        {
            return StatusCode(StatusCodes.Status410Gone, new ProblemDetails { Title = "Pairing code expired.", Detail = $"Pairing code '{code}' has expired.", Status = StatusCodes.Status410Gone });
        }

        var venue = await venueRepository.GetByIdAsync(request.VenueId, cancellationToken);

        if (venue is null)
        {
            return NotFound(new ProblemDetails { Title = "Venue not found.", Detail = $"Venue '{request.VenueId}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        var screen = await screenRepository.GetByIdAsync(pairingCode.ScreenId, cancellationToken);

        if (screen is null)
        {
            return NotFound(new ProblemDetails { Title = "Screen not found.", Detail = $"Screen '{pairingCode.ScreenId}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        var claimed = await screenPairingCodeRepository.ClaimAsync(code, request.VenueId, cancellationToken);

        if (!claimed)
        {
            return Conflict(new ProblemDetails { Title = "Pairing code could not be claimed.", Detail = $"Pairing code '{code}' could not be claimed.", Status = StatusCodes.Status409Conflict });
        }

        var linked = await screenRepository.AssignVenueAsync(pairingCode.ScreenId, request.VenueId, cancellationToken);

        if (!linked)
        {
            return Problem(title: "Screen could not be linked.", detail: $"Screen '{pairingCode.ScreenId}' could not be linked to venue '{request.VenueId}'.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var response = new ClaimScreenPairingCodeResponse
        {
            Linked = true,
            ScreenId = pairingCode.ScreenId,
            VenueId = request.VenueId
        };

        return Ok(response);
    }

    private async Task<string> GenerateUniqueScreenKeyAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var screenKey = IdentifierGenerator.CreateScreenKey();

            if (await screenRepository.GetByScreenKeyAsync(screenKey, cancellationToken) is null)
            {
                return screenKey;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique screen key.");
    }

    private async Task<string> GenerateUniquePairingCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var pairingCode = IdentifierGenerator.CreatePairingCode();

            if (await screenPairingCodeRepository.GetByCodeAsync(pairingCode, cancellationToken) is null)
            {
                return pairingCode;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique pairing code.");
    }
}

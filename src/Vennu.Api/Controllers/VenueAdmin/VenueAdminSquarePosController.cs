using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.VenueAdmin;
using Vennu.Api.Pos;
using Vennu.Api.VenueAdmin;
using Vennu.Core.Models;

namespace Vennu.Api.Controllers.VenueAdmin;

[ApiController]
[Route("api/venue-admin/pos/square")]
[Authorize(Policy = VenueAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class VenueAdminSquarePosController(ISquareOAuthConnectionService service) : ControllerBase
{
    [HttpPost("connect")]
    public ActionResult<VenueAdminPosConnectResponse> Connect() =>
        Ok(new VenueAdminPosConnectResponse(service.Begin(VenueId()).AbsoluteUri));

    [HttpGet("status")]
    public async Task<ActionResult<VenueAdminPosConnectionResponse?>> Status(CancellationToken cancellationToken)
    {
        var connection = (await service.GetStatusAsync(VenueId(), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(value => value.Provider == PosProvider.Square);
        return Ok(connection is null ? null : new VenueAdminPosConnectionResponse(
            "square", connection.Status.ToString().ToLowerInvariant(), connection.ExternalMerchantId,
            connection.AccessTokenExpiresUtc, connection.UpdatedUtc));
    }

    [HttpDelete("connection")]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken) =>
        await service.DisconnectAsync(VenueId(), cancellationToken).ConfigureAwait(false) ? NoContent() : NotFound();

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code, [FromQuery] string state, [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error)) return Redirect(service.ReturnUri("square-denied").AbsoluteUri);
        if (string.IsNullOrWhiteSpace(code)) return Redirect(service.ReturnUri("square-error").AbsoluteUri);
        try
        {
            await service.CompleteAsync(state, code, cancellationToken).ConfigureAwait(false);
            return Redirect(service.ReturnUri("square-connected").AbsoluteUri);
        }
        catch (InvalidOperationException)
        {
            return Redirect(service.ReturnUri("square-error").AbsoluteUri);
        }
    }

    private Guid VenueId() => Guid.Parse(User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
}

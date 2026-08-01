using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.VenueAdmin;
using Vennu.Api.Pos;
using Vennu.Api.VenueAdmin;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.VenueAdmin;

[ApiController]
[Route("api/venue-admin/pos/clover")]
[Authorize(Policy = VenueAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class VenueAdminCloverPosController(
    ICloverOAuthConnectionService service,
    IPosCatalogImportService catalogImportService) : ControllerBase
{
    [HttpPost("connect")]
    public ActionResult<VenueAdminPosConnectResponse> Connect() =>
        Ok(new VenueAdminPosConnectResponse(service.Begin(VenueId()).AbsoluteUri));

    [HttpGet("status")]
    public async Task<ActionResult<VenueAdminPosConnectionResponse?>> Status(CancellationToken cancellationToken)
    {
        var connection = (await service.GetStatusAsync(VenueId(), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(value => value.Provider == PosProvider.Clover);
        return Ok(connection is null ? null : new VenueAdminPosConnectionResponse(
            "clover",
            connection.Status.ToString().ToLowerInvariant(),
            connection.ExternalMerchantId,
            connection.AccessTokenExpiresUtc,
            connection.UpdatedUtc));
    }

    [HttpDelete("connection")]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken) =>
        await service.DisconnectAsync(VenueId(), cancellationToken).ConfigureAwait(false) ? NoContent() : NotFound();

    [HttpPost("catalog/import")]
    public async Task<ActionResult<PosCatalogImportResult>> ImportCatalog(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await catalogImportService.ImportAsync(VenueId(), PosProvider.Clover, cancellationToken).ConfigureAwait(false));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Clover catalog import is unavailable",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string state,
        [FromQuery(Name = "merchant_id")] string? merchantId,
        [FromQuery(Name = "client_id")] string? clientId,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error)) return Redirect(service.ReturnUri("clover-denied").AbsoluteUri);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(merchantId) || string.IsNullOrWhiteSpace(clientId))
            return Redirect(service.ReturnUri("clover-error").AbsoluteUri);
        try
        {
            await service.CompleteAsync(state, code, merchantId, clientId, cancellationToken).ConfigureAwait(false);
            return Redirect(service.ReturnUri("clover-connected").AbsoluteUri);
        }
        catch (InvalidOperationException)
        {
            return Redirect(service.ReturnUri("clover-error").AbsoluteUri);
        }
    }

    private Guid VenueId() => Guid.Parse(User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
}

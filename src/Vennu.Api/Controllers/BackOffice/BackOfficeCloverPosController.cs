using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.Pos;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/pos/clover")]
[Route("api/venue-admin/pos/clover")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[RequireCapability("content.source.synchronize")]
public sealed class BackOfficeCloverPosController(
    ICloverOAuthConnectionService service,
    IPosCatalogImportService catalogImportService) : ControllerBase
{
    [HttpPost("connect")]
    public ActionResult<BackOfficePosConnectResponse> Connect() =>
        Ok(new BackOfficePosConnectResponse(service.Begin(VenueId()).AbsoluteUri));

    [HttpGet("status")]
    public async Task<ActionResult<BackOfficeCloverStatusResponse>> Status(CancellationToken cancellationToken)
    {
        var connection = (await service.GetStatusAsync(VenueId(), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(value => value.Provider == PosProvider.Clover);
        var summary = connection is null ? null : new BackOfficePosConnectionResponse(
            "clover", connection.Status.ToString().ToLowerInvariant(), connection.ExternalMerchantId,
            connection.AccessTokenExpiresUtc, connection.UpdatedUtc);
        return Ok(new BackOfficeCloverStatusResponse(
            summary,
            "external_configuration_required",
            true,
            "Register the HTTPS Clover webhook URL in the Clover Developer Dashboard, complete its verification-code step through an operator-controlled receiver, subscribe to inventory events, and configure the resulting X-Clover-Auth key. Vennusign does not claim registration until Clover confirms it.",
            connection?.LastSyncedUtc,
            connection?.LastSyncAttemptUtc,
            connection?.ConsecutiveSyncFailures ?? 0,
            connection?.LastSyncErrorCode));
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

    private Guid VenueId() => Guid.Parse(User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);
}

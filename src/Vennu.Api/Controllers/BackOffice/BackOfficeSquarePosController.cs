using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.Pos;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/pos/square")]
[Route("api/venue-admin/pos/square")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[RequireCapability("content.source.synchronize")]
public sealed class BackOfficeSquarePosController(
    ISquareOAuthConnectionService service,
    Vennu.Data.Services.IPosCatalogImportService catalogImportService) : ControllerBase
{
    [HttpPost("connect")]
    public ActionResult<BackOfficePosConnectResponse> Connect() =>
        Ok(new BackOfficePosConnectResponse(service.Begin(VenueId()).AbsoluteUri));

    [HttpGet("status")]
    public async Task<ActionResult<BackOfficePosConnectionResponse?>> Status(CancellationToken cancellationToken)
    {
        var connection = (await service.GetStatusAsync(VenueId(), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(value => value.Provider == PosProvider.Square);
        return Ok(connection is null ? null : new BackOfficePosConnectionResponse(
            "square", connection.Status.ToString().ToLowerInvariant(), connection.ExternalMerchantId,
            connection.AccessTokenExpiresUtc, connection.UpdatedUtc));
    }

    [HttpDelete("connection")]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken) =>
        await service.DisconnectAsync(VenueId(), cancellationToken).ConfigureAwait(false) ? NoContent() : NotFound();

    [HttpPost("catalog/import")]
    public async Task<ActionResult<Vennu.Data.Services.PosCatalogImportResult>> ImportCatalog(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await catalogImportService.ImportAsync(VenueId(), cancellationToken).ConfigureAwait(false));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Square catalog import is unavailable",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

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

    private Guid VenueId() => Guid.Parse(User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);
}

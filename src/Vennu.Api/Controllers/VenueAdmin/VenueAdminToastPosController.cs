using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.VenueAdmin;
using Vennu.Api.VenueAdmin;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.VenueAdmin;

[ApiController]
[Route("api/venue-admin/pos/toast")]
[Authorize(Policy = VenueAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class VenueAdminToastPosController(
    IPosConnectionService connections,
    IPosCatalogImportService catalogImport) : ControllerBase
{
    [HttpPut("connection")]
    public async Task<ActionResult<VenueAdminPosConnectionResponse>> Configure(
        ConfigureToastConnectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.RestaurantGuid, out var restaurantGuid) || string.IsNullOrWhiteSpace(request.AccessToken))
            return ValidationProblem("A Toast restaurant GUID and access token are required.");
        var saved = await connections.StoreCredentialsAsync(
            VenueId(), PosProvider.Toast, restaurantGuid.ToString(),
            new PosCredentialInput(request.AccessToken, null, null), cancellationToken).ConfigureAwait(false);
        return Ok(ToResponse(saved));
    }

    [HttpGet("status")]
    public async Task<ActionResult<VenueAdminToastStatusResponse>> Status(CancellationToken cancellationToken)
    {
        var connection = (await connections.GetAllAsync(VenueId(), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(value => value.Provider == PosProvider.Toast);
        return Ok(new VenueAdminToastStatusResponse(
            connection is null ? null : ToResponse(connection),
            "manual_provider_registration_required",
            true,
            "Register the HTTPS menus and stock webhook endpoint in the Toast developer portal after Toast approves the integration. Vennu does not claim registration until Toast confirms it.",
            connection is null ? null : new VenueAdminToastPollingHealthResponse(
                PollingState(connection),
                connection.LastSyncAttemptUtc,
                connection.LastSyncedUtc,
                connection.ConsecutiveSyncFailures,
                connection.NextSyncAttemptUtc,
                connection.LastSyncErrorCode)));
    }

    [HttpPost("catalog/import")]
    public async Task<ActionResult<PosCatalogImportResult>> ImportCatalog(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await catalogImport.ImportAsync(VenueId(), PosProvider.Toast, cancellationToken).ConfigureAwait(false));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Toast catalog import is unavailable",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    private static VenueAdminPosConnectionResponse ToResponse(PosConnectionSummary connection) =>
        new("toast", connection.Status.ToString().ToLowerInvariant(), connection.ExternalMerchantId,
            connection.AccessTokenExpiresUtc, connection.UpdatedUtc);

    private static string PollingState(PosConnectionSummary connection) =>
        connection.Status == PosConnectionStatus.ReauthorizationRequired ? "reauthorization_required" :
        connection.ConsecutiveSyncFailures > 0 ? "retry_scheduled" :
        connection.LastSyncedUtc.HasValue ? "healthy" : "pending";

    private Guid VenueId() => Guid.Parse(User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
}

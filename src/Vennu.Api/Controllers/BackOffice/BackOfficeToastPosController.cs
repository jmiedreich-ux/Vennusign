using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/pos/toast")]
[Route("api/venue-admin/pos/toast")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
public sealed class BackOfficeToastPosController(
    IPosConnectionService connections,
    IPosCatalogImportService catalogImport) : ControllerBase
{
    [HttpPut("connection")]
    public async Task<ActionResult<BackOfficePosConnectionResponse>> Configure(
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
    public async Task<ActionResult<BackOfficeToastStatusResponse>> Status(CancellationToken cancellationToken)
    {
        var connection = (await connections.GetAllAsync(VenueId(), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(value => value.Provider == PosProvider.Toast);
        return Ok(new BackOfficeToastStatusResponse(
            connection is null ? null : ToResponse(connection),
            "manual_provider_registration_required",
            true,
            "Register the HTTPS menus and stock webhook endpoint in the Toast developer portal after Toast approves the integration. Vennusign does not claim registration until Toast confirms it.",
            connection is null ? null : new BackOfficeToastPollingHealthResponse(
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

    private static BackOfficePosConnectionResponse ToResponse(PosConnectionSummary connection) =>
        new("toast", connection.Status.ToString().ToLowerInvariant(), connection.ExternalMerchantId,
            connection.AccessTokenExpiresUtc, connection.UpdatedUtc);

    private static string PollingState(PosConnectionSummary connection) =>
        connection.Status == PosConnectionStatus.ReauthorizationRequired ? "reauthorization_required" :
        connection.ConsecutiveSyncFailures > 0 ? "retry_scheduled" :
        connection.LastSyncedUtc.HasValue ? "healthy" : "pending";

    private Guid VenueId() => Guid.Parse(User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);
}

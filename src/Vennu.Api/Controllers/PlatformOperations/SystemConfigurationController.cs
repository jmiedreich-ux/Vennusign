using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Data.Configuration;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/configuration")]
[Route("api/admin/configuration")]
public sealed class SystemConfigurationController(
    ISystemConfigurationService service,
    ISystemConfigurationOperationsService operations,
    SystemConfigurationProviderHealth providerHealth) : ControllerBase
{
    [HttpGet("health")]
    [Authorize(Policy = "Configuration:read")]
    public ActionResult<SystemConfigurationProviderHealthSnapshot> Health() => Ok(providerHealth.Snapshot());

    [HttpGet]
    [Authorize(Policy = "Configuration:read")]
    public async Task<ActionResult<IReadOnlyList<SystemConfigurationSettingResponse>>> Get(
        [FromQuery] string environmentName,
        [FromQuery] string? applicationScope,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await service.GetAsync(environmentName, applicationScope, cancellationToken).ConfigureAwait(false);
            return Ok(settings.Select(ToResponse).ToArray());
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPut("{definitionId:guid}")]
    [Authorize(Policy = "Configuration:edit")]
    public Task<ActionResult<SystemConfigurationSettingResponse>> Set(
        Guid definitionId,
        SystemConfigurationWriteRequest request,
        CancellationToken cancellationToken) => ExecuteAsync(definitionId, request, false, cancellationToken);

    [HttpDelete("{definitionId:guid}")]
    [Authorize(Policy = "Configuration:edit")]
    public Task<ActionResult<SystemConfigurationSettingResponse>> Clear(
        Guid definitionId,
        SystemConfigurationWriteRequest request,
        CancellationToken cancellationToken) => ExecuteAsync(definitionId, request, true, cancellationToken);

    [HttpGet("{definitionId:guid}/revisions")]
    [Authorize(Policy = "Configuration:read")]
    public async Task<ActionResult<IReadOnlyList<SystemConfigurationRevision>>> Revisions(
        Guid definitionId,
        [FromQuery] string environmentName,
        CancellationToken cancellationToken) =>
        Ok(await operations.GetRevisionsAsync(definitionId, environmentName, cancellationToken).ConfigureAwait(false));

    [HttpPost("{definitionId:guid}/rollback")]
    [Authorize(Policy = "Configuration:admin")]
    public async Task<IActionResult> Rollback(
        Guid definitionId,
        SystemConfigurationRollbackRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await operations.RollbackAsync(new(
                definitionId, request.EnvironmentName, request.RevisionNumber, request.ExpectedVersion,
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "platform-operations"), cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
        catch (DBConcurrencyException exception)
        {
            return Conflict(new ProblemDetails { Title = "Configuration changed", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    private async Task<ActionResult<SystemConfigurationSettingResponse>> ExecuteAsync(
        Guid definitionId,
        SystemConfigurationWriteRequest request,
        bool clear,
        CancellationToken cancellationToken)
    {
        try
        {
            var current = (await service.GetAsync(request.EnvironmentName, null, cancellationToken).ConfigureAwait(false))
                .SingleOrDefault(setting => setting.DefinitionId == definitionId);
            if (current is null) return NotFound();
            if (current.IsSecret && !User.HasClaim("vennusign:configuration_permission", "secrets")) return Forbid();
            var write = new SystemConfigurationWrite(
                definitionId,
                request.EnvironmentName,
                request.Value,
                request.ExpectedVersion,
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "platform-operations",
                "PlatformOperations");
            var result = clear
                ? await service.ClearAsync(write, cancellationToken).ConfigureAwait(false)
                : await service.SetAsync(write, cancellationToken).ConfigureAwait(false);
            return Ok(ToResponse(result));
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (DBConcurrencyException exception)
        {
            return Conflict(new ProblemDetails { Title = "Configuration changed", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static SystemConfigurationSettingResponse ToResponse(SystemConfigurationSetting setting) => new(
        setting.DefinitionId, setting.Key, setting.ApplicationScope, setting.Description, setting.ValueType,
        setting.IsRequired, setting.IsSecret, setting.Value, setting.HasConfiguredValue, setting.RequiresRestart,
        setting.ExportPolicy, setting.Version, setting.LastUpdatedUtc, setting.RotationReminderDays);
}

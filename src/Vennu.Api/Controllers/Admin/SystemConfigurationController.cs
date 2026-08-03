using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.Admin;
using Vennu.Data.Configuration;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/configuration")]
public sealed class SystemConfigurationController(ISystemConfigurationService service) : ControllerBase
{
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
            if (current.IsSecret && !User.HasClaim("vennu:configuration_permission", "secrets")) return Forbid();
            var write = new SystemConfigurationWrite(
                definitionId,
                request.EnvironmentName,
                request.Value,
                request.ExpectedVersion,
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "super-admin",
                "SuperAdmin");
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
        setting.ExportPolicy, setting.Version);
}

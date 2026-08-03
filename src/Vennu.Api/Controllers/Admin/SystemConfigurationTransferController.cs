using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.Admin;
using Vennu.Data.Configuration;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/configuration-transfer")]
[Authorize(Policy = "Configuration:import")]
public sealed class SystemConfigurationTransferController(ISystemConfigurationTransferService service) : ControllerBase
{
    [HttpGet("export")]
    public async Task<ActionResult<SystemConfigurationManifest>> Export([FromQuery] string environmentName, CancellationToken cancellationToken)
    {
        try { return Ok(await service.ExportAsync(environmentName, cancellationToken).ConfigureAwait(false)); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPost("preview")]
    public async Task<ActionResult<SystemConfigurationImportPreview>> Preview(SystemConfigurationImportPreviewRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await service.PreviewAsync(request.TargetEnvironment, request.Manifest, cancellationToken).ConfigureAwait(false)); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply(SystemConfigurationImportApplyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await service.ApplyAsync(new(
                request.OperationId,
                request.TargetEnvironment,
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "super-admin",
                request.Settings), cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
        catch (DBConcurrencyException exception)
        {
            return Conflict(new ProblemDetails { Title = "Import preview is stale", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }
}

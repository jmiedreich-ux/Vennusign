using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/features")]
[Route("api/admin/features")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsFeaturesController(IFeatureMatrixService service) : ControllerBase
{
    [HttpGet]
    public Task<FeatureMatrixSnapshot> Get(CancellationToken cancellationToken) =>
        service.GetAsync(cancellationToken);

    [HttpPut]
    public async Task<ActionResult<FeatureMatrixUpdateResponse>> Update(
        FeatureMatrixUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Changes is null)
            {
                return ValidationProblem("At least an empty changes collection is required.");
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.Identity?.Name
                ?? "platform-operations";
            var changes = request.Changes
                .Select(change => new FeatureMatrixChange(change.TierId, change.FeatureId, change.Enabled))
                .ToArray();
            var changedCount = await service.ApplyAsync(changes, adminId, cancellationToken).ConfigureAwait(false);
            return Ok(new FeatureMatrixUpdateResponse(changedCount));
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }
}

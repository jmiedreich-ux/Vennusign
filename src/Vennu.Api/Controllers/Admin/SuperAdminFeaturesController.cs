using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/features")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminFeaturesController(IFeatureMatrixService service) : ControllerBase
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
                ?? "super-admin";
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

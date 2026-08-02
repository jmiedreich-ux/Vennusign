using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/onboarding")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminOnboardingController(ISuperAdminOnboardingRepository onboarding) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<SuperAdminOnboardingRecord>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<SuperAdminOnboardingRecord>>> Get(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await onboarding.SearchAsync(search, cancellationToken).ConfigureAwait(false));
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/onboarding")]
[Route("api/admin/onboarding")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsOnboardingController(IPlatformOperationsOnboardingRepository onboarding) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<PlatformOperationsOnboardingRecord>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<PlatformOperationsOnboardingRecord>>> Get(
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

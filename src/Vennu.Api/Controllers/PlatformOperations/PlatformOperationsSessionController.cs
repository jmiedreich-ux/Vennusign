using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Api.Contracts.PlatformOperations;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/session")]
[Route("api/admin/session")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsSessionController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PlatformOperationsSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<PlatformOperationsSessionResponse> Get()
    {
        return Ok(new PlatformOperationsSessionResponse(
            User.Identity?.Name ?? "Platform Operations",
            ["dashboard", "venues", "tiers", "features"]));
    }
}

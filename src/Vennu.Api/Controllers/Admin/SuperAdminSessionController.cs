using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/session")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminSessionController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<SuperAdminSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<SuperAdminSessionResponse> Get()
    {
        return Ok(new SuperAdminSessionResponse(
            User.Identity?.Name ?? "Super Admin",
            ["dashboard", "venues", "tiers", "features"]));
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminDashboardController(IOperationalDashboardService service) : ControllerBase
{
    [HttpGet]
    public Task<OperationalDashboard> Get(CancellationToken cancellationToken) =>
        service.GetAsync(cancellationToken);
}

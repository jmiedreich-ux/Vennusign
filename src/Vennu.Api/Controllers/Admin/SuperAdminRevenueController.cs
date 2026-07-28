using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard/revenue")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminRevenueController(IRevenueSnapshotService service) : ControllerBase
{
    [HttpGet]
    public Task<RevenueSnapshot> Get(CancellationToken cancellationToken) =>
        service.GetAsync(cancellationToken);
}

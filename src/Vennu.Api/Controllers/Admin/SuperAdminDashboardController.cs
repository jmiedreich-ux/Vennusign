using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminDashboardController(
    IOperationalDashboardService service,
    IOperationalEventFeedService eventFeedService) : ControllerBase
{
    [HttpGet]
    public Task<OperationalDashboard> Get(CancellationToken cancellationToken) =>
        service.GetAsync(cancellationToken);

    [HttpGet("events")]
    public Task<IReadOnlyCollection<OperationalEventFeedItem>> GetEvents(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default) =>
        eventFeedService.GetRecentAsync(limit, cancellationToken);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/dashboard")]
[Route("api/admin/dashboard")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsDashboardController(
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

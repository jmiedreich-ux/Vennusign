using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/dashboard/revenue")]
[Route("api/admin/dashboard/revenue")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsRevenueController(
    IRevenueSnapshotService service,
    IRevenueTrendService trendService) : ControllerBase
{
    [HttpGet]
    public async Task<RevenueSnapshot> Get(CancellationToken cancellationToken)
    {
        var snapshot = await service.GetAsync(cancellationToken).ConfigureAwait(false);
        await trendService.CaptureAsync(snapshot, cancellationToken).ConfigureAwait(false);
        return snapshot;
    }

    [HttpGet("trend")]
    public Task<RevenueTrend> GetTrend(
        [FromQuery, Range(1, 24)] int months = 12,
        CancellationToken cancellationToken = default) =>
        trendService.GetAsync(months, cancellationToken);
}

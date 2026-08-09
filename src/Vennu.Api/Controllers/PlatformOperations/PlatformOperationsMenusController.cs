using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.PlatformOperations;

/// <summary>
/// Platform operations can look at a venue's menus, and nothing else. The old
/// direct-edit endpoints are retired (Q36): an ops edit wrote straight to live
/// tables, so a screen could change without a publish. The intended ops model —
/// impersonation with customer consent — is backlogged; until it exists, ops has
/// no menu write path.
/// </summary>
[ApiController]
[Route("api/platform-operations/venues/{venueId:guid}/menus")]
[Route("api/admin/venues/{venueId:guid}/menus")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsMenusController(
    IMenuSectionManagementService sectionService) : ControllerBase
{
    [HttpGet]
    public Task<MenuEditorSnapshot> Get(Guid venueId, CancellationToken cancellationToken) =>
        sectionService.GetAsync(venueId, cancellationToken);
}

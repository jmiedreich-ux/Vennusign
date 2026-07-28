using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/tiers")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminTiersController(ITierManagementService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<SubscriptionTier>> Get(CancellationToken cancellationToken) =>
        service.GetAllAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<SubscriptionTier>> Create(TierManagementRequest request, CancellationToken cancellationToken)
    {
        try { return Created(string.Empty, await service.CreateAsync(request, cancellationToken).ConfigureAwait(false)); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(new ProblemDetails { Detail = exception.Message }); }
    }

    [HttpPut("{tierId:guid}")]
    public async Task<ActionResult<SubscriptionTier>> Update(Guid tierId, TierManagementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tier = await service.UpdateAsync(tierId, request, cancellationToken).ConfigureAwait(false);
            return tier is null ? NotFound() : Ok(tier);
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(new ProblemDetails { Detail = exception.Message }); }
    }

    [HttpPost("{tierId:guid}/clone")]
    public async Task<ActionResult<SubscriptionTier>> Clone(Guid tierId, CancellationToken cancellationToken)
    {
        var tier = await service.CloneAsync(tierId, cancellationToken).ConfigureAwait(false);
        return tier is null ? NotFound() : Created(string.Empty, tier);
    }

    [HttpPost("{tierId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid tierId, CancellationToken cancellationToken) =>
        await service.ArchiveAsync(tierId, cancellationToken).ConfigureAwait(false) ? NoContent() : NotFound();
}

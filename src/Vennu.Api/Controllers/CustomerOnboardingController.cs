using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.CustomerOnboarding;
using Vennu.Api.CustomerAuthentication;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/customer-onboarding")]
public sealed class CustomerOnboardingController(ICustomerOnboardingService onboarding) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyCollection<PublicOnboardingPlan>>> GetPlans(
        CancellationToken cancellationToken) =>
        Ok(await onboarding.GetPublicPlansAsync(cancellationToken).ConfigureAwait(false));

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpGet]
    public async Task<ActionResult<CustomerOnboardingSnapshot>> Get(CancellationToken cancellationToken) =>
        Ok(await onboarding.GetAsync(UserId(), cancellationToken).ConfigureAwait(false));

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("organization")]
    public async Task<ActionResult<CustomerOnboardingSnapshot>> CreateOrganization(
        CreateOnboardingOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await onboarding.CreateOrganizationAsync(
                UserId(), request.Name, cancellationToken).ConfigureAwait(false));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("trial")]
    public async Task<ActionResult<CustomerOnboardingSnapshot>> StartTrial(
        SelectOnboardingTrialRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await onboarding.StartTrialAsync(UserId(), request.TierId, cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("checkout")]
    public async Task<ActionResult<CreateOnboardingCheckoutResponse>> CreateCheckout(
        CreateOnboardingCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CheckoutBillingInterval>(request.BillingInterval, true, out var interval) ||
            !Enum.IsDefined(interval))
            return BadRequest("Billing interval must be monthly or annual.");
        try
        {
            var result = await onboarding.CreateCheckoutAsync(
                UserId(), request.TierId, interval, cancellationToken).ConfigureAwait(false);
            return Ok(new CreateOnboardingCheckoutResponse(result.CheckoutUrl.AbsoluteUri));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.VenueAdmin;
using Vennu.Api.VenueAdmin;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.VenueAdmin;

[ApiController]
[Route("api/venue-admin/billing")]
[Authorize(Policy = VenueAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class VenueAdminBillingController(
    IVenueSupportDetailService supportDetailService,
    ISubscriptionTierRepository tierRepository,
    ICheckoutSessionService checkoutSessionService,
    IBillingPortalSessionService billingPortalSessionService) : ControllerBase
{
    [HttpGet("presentation")]
    [ProducesResponseType<VenueAdminBillingPresentationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VenueAdminBillingPresentationResponse>> GetPresentation(
        CancellationToken cancellationToken)
    {
        var venueId = Guid.Parse(
            User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
        var detail = await supportDetailService
            .GetAsync(venueId, cancellationToken)
            .ConfigureAwait(false);
        if (detail is null)
        {
            return NotFound();
        }

        var tiers = await tierRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new VenueAdminBillingPresentationResponse(
            detail.Tier is null ? null : ToSummary(detail.Tier),
            detail.Subscription is null ? null : new VenueAdminSubscriptionSummary(
                detail.Subscription.Status,
                detail.Subscription.TrialEndsAt,
                detail.Subscription.CurrentPeriodEnd,
                detail.Subscription.CancelAtPeriodEnd,
                !string.IsNullOrWhiteSpace(detail.Subscription.StripeSubscriptionId) &&
                    !string.Equals(detail.Subscription.Status, "canceled", StringComparison.OrdinalIgnoreCase)),
            tiers
                .Where(tier => tier.IsActive && tier.IsPublic)
                .OrderBy(tier => tier.Price)
                .ThenBy(tier => tier.Name, StringComparer.OrdinalIgnoreCase)
                .Select(ToSummary)
                .ToArray(),
            detail.Features.ToDictionary(
                pair => pair.Key,
                pair => new VenueAdminFeatureSummary(
                    pair.Value.Enabled,
                    pair.Value.LimitValue),
                StringComparer.OrdinalIgnoreCase)));
    }

    [HttpPost("portal-session")]
    [ProducesResponseType<CreateBillingPortalSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateBillingPortalSessionResponse>> CreateBillingPortalSession(
        CancellationToken cancellationToken)
    {
        var venueId = Guid.Parse(
            User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
        try
        {
            var result = await billingPortalSessionService
                .CreateAsync(venueId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(new CreateBillingPortalSessionResponse(result.PortalUrl.AbsoluteUri));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Billing management could not be opened.",
                Detail = exception.Message
            });
        }
    }

    [HttpPost("checkout-session")]
    [ProducesResponseType<CreateCheckoutSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateCheckoutSessionResponse>> CreateCheckoutSession(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CheckoutBillingInterval>(
                request.BillingInterval,
                ignoreCase: true,
                out var billingInterval) ||
            !Enum.IsDefined(billingInterval))
        {
            return ValidationProblem("Billing interval must be monthly or annual.");
        }

        var venueId = Guid.Parse(
            User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
        try
        {
            var result = await checkoutSessionService.CreateAsync(
                venueId,
                request.TargetTierId,
                billingInterval,
                cancellationToken).ConfigureAwait(false);
            return Ok(new CreateCheckoutSessionResponse(result.CheckoutUrl.AbsoluteUri));
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Checkout session could not be created.",
                Detail = exception.Message
            });
        }
    }

    private static VenueAdminTierSummary ToSummary(SubscriptionTier tier) =>
        new(tier.Id, tier.Name, tier.Slug, tier.Price, tier.MaxScreens);
}

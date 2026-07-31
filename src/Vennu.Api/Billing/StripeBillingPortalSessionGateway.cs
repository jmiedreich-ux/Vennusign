using Microsoft.Extensions.Options;
using Stripe;
using Vennu.Data.Services;

namespace Vennu.Api.Billing;

public sealed class StripeBillingPortalSessionGateway(
    IOptions<StripeRevenueOptions> revenueOptions,
    IOptions<StripeBillingPortalOptions> portalOptions) : IStripeBillingPortalSessionGateway
{
    public async Task<StripeBillingPortalSessionResult> CreateAsync(
        StripeBillingPortalSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = revenueOptions.Value.ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Stripe Billing Portal API access is not configured.");
        }

        var returnUrl = RequireHttpsUrl(portalOptions.Value.ReturnUrl);
        var client = new StripeClient(apiKey);
        var subscription = await new SubscriptionService(client)
            .GetAsync(request.StripeSubscriptionId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            throw new InvalidOperationException("The Stripe subscription does not have a customer for billing management.");
        }

        var session = await new Stripe.BillingPortal.SessionService(client)
            .CreateAsync(
                new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = subscription.CustomerId,
                    ReturnUrl = returnUrl.AbsoluteUri
                },
                requestOptions: null,
                cancellationToken)
            .ConfigureAwait(false);
        if (!Uri.TryCreate(session.Url, UriKind.Absolute, out var portalUrl) ||
            portalUrl.Scheme != Uri.UriSchemeHttps ||
            !portalUrl.Host.Equals("billing.stripe.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Stripe did not return an allowlisted hosted Billing Portal URL.");
        }

        return new StripeBillingPortalSessionResult(portalUrl);
    }

    private static Uri RequireHttpsUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var result) ||
            result.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("Stripe Billing Portal return URL must be an absolute HTTPS URL.");
        }

        return result;
    }
}

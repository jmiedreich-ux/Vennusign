using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Vennu.Data.Services;

namespace Vennu.Api.Billing;

public sealed class StripeCheckoutSessionGateway(
    IOptions<StripeRevenueOptions> revenueOptions,
    IOptions<StripeCheckoutOptions> checkoutOptions) : IStripeCheckoutSessionGateway
{
    public async Task<StripeCheckoutSessionResult> CreateAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = revenueOptions.Value.ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Stripe Checkout API access is not configured.");
        }

        var successUrl = RequireHttpsUrl(checkoutOptions.Value.SuccessUrl, "success");
        var cancelUrl = RequireHttpsUrl(checkoutOptions.Value.CancelUrl, "cancel");
        var service = new SessionService(new StripeClient(apiKey));
        var ownerId = request.OrganizationId ?? request.VenueId;
        if (ownerId == Guid.Empty)
            throw new InvalidOperationException("Stripe Checkout requires an organization or legacy venue owner.");
        var metadata = new Dictionary<string, string>
        {
            [request.OrganizationId is null ? "venue_id" : "organization_id"] = ownerId.ToString(),
            ["tier_slug"] = request.TierSlug
        };
        var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = successUrl.AbsoluteUri,
                CancelUrl = cancelUrl.AbsoluteUri,
                ClientReferenceId = ownerId.ToString(),
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = request.PriceId,
                        Quantity = 1
                    }
                ],
                Metadata = metadata,
                SubscriptionData = new SessionSubscriptionDataOptions { Metadata = metadata }
            };
        if (!string.IsNullOrWhiteSpace(request.StripeCustomerId))
            options.Customer = request.StripeCustomerId.Trim();
        var session = await service.CreateAsync(
            options,
            requestOptions: null,
            cancellationToken).ConfigureAwait(false);

        if (!Uri.TryCreate(session.Url, UriKind.Absolute, out var checkoutUrl) ||
            checkoutUrl.Scheme != Uri.UriSchemeHttps ||
            !checkoutUrl.Host.Equals("checkout.stripe.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Stripe did not return an allowlisted hosted Checkout URL.");
        }

        return new StripeCheckoutSessionResult(checkoutUrl);
    }

    private static Uri RequireHttpsUrl(string value, string name)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var result) ||
            result.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException($"Stripe Checkout {name} URL must be an absolute HTTPS URL.");
        }

        return result;
    }
}

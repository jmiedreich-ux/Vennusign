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
        var session = await service.CreateAsync(
            new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = successUrl.AbsoluteUri,
                CancelUrl = cancelUrl.AbsoluteUri,
                ClientReferenceId = request.VenueId.ToString(),
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = request.PriceId,
                        Quantity = 1
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["venue_id"] = request.VenueId.ToString(),
                    ["tier_slug"] = request.TierSlug
                }
            },
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

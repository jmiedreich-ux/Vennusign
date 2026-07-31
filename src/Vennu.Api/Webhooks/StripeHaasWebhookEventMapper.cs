using Vennu.Data.Services;

namespace Vennu.Api.Webhooks;

public static class StripeHaasWebhookEventMapper
{
    private const string Created = "customer.subscription.created";
    private const string Updated = "customer.subscription.updated";
    private const string Deleted = "customer.subscription.deleted";

    public static bool TryMap(Stripe.Event stripeEvent, out HaasContractSubscriptionEvent? contractEvent)
    {
        ArgumentNullException.ThrowIfNull(stripeEvent);
        contractEvent = null;
        if (stripeEvent.Type is not (Created or Updated or Deleted) ||
            stripeEvent.Data?.Object is not Stripe.Subscription subscription ||
            subscription.Metadata is null ||
            !subscription.Metadata.TryGetValue("haas_bundle_key", out var bundleKey))
        {
            return false;
        }

        var eventId = Required(stripeEvent.Id, "Stripe event ID is required.");
        var subscriptionId = Required(subscription.Id, "Stripe subscription ID is required.");
        if (stripeEvent.Type == Deleted)
        {
            contractEvent = new HaasContractSubscriptionEvent(eventId, stripeEvent.Type, subscriptionId);
            return true;
        }

        if (!subscription.Metadata.TryGetValue("venue_id", out var venueValue) ||
            !Guid.TryParse(venueValue, out var venueId) || venueId == Guid.Empty ||
            !subscription.Metadata.TryGetValue("haas_term_months", out var termValue) ||
            !int.TryParse(termValue, out var termMonths) ||
            subscription.StartDate <= DateTime.UnixEpoch)
        {
            throw new StripeWebhookPayloadException("HaaS subscription metadata and start date are invalid.");
        }

        contractEvent = new HaasContractSubscriptionEvent(
            eventId,
            stripeEvent.Type,
            subscriptionId,
            venueId,
            bundleKey,
            termMonths,
            Required(subscription.Status, "Stripe subscription status is required."),
            subscription.StartDate.ToUniversalTime(),
            subscription.CancelAtPeriodEnd);
        return true;
    }

    private static string Required(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StripeWebhookPayloadException(message);
        }

        return value.Trim();
    }
}

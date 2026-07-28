using Vennu.Data.Services;

namespace Vennu.Api.Webhooks;

public static class StripeWebhookEventMapper
{
    private const string VenueIdMetadataKey = "venue_id";
    private const string SubscriptionCreated = "customer.subscription.created";
    private const string SubscriptionUpdated = "customer.subscription.updated";
    private const string SubscriptionDeleted = "customer.subscription.deleted";
    private const string InvoicePaid = "invoice.paid";

    public static bool TryMap(
        Stripe.Event stripeEvent,
        out StripeSubscriptionEvent? subscriptionEvent)
    {
        ArgumentNullException.ThrowIfNull(stripeEvent);

        subscriptionEvent = stripeEvent.Type switch
        {
            SubscriptionCreated or SubscriptionUpdated => MapSubscription(stripeEvent),
            SubscriptionDeleted => MapDeletedSubscription(stripeEvent),
            InvoicePaid => MapPaidInvoice(stripeEvent),
            _ => null
        };

        return subscriptionEvent is not null;
    }

    private static StripeSubscriptionEvent MapSubscription(Stripe.Event stripeEvent)
    {
        var subscription = GetObject<Stripe.Subscription>(stripeEvent);
        var venueId = GetVenueId(subscription.Metadata);
        var activeItems = subscription.Items?.Data?
            .Where(item => item.Deleted is not true)
            .ToArray() ?? [];
        if (activeItems.Length != 1)
        {
            throw new StripeWebhookPayloadException(
                "Stripe subscription payload must contain exactly one active tier item.");
        }

        var item = activeItems[0];
        var priceId = Required(item.Price?.Id, "Stripe subscription item price ID is required.");
        var currentPeriodEnd = item.CurrentPeriodEnd <= DateTime.UnixEpoch
            ? null
            : item.CurrentPeriodEnd.ToUniversalTime();

        return new StripeSubscriptionEvent(
            Required(stripeEvent.Id, "Stripe event ID is required."),
            Required(stripeEvent.Type, "Stripe event type is required."),
            Required(subscription.Id, "Stripe subscription ID is required."),
            venueId,
            priceId,
            Required(subscription.Status, "Stripe subscription status is required."),
            subscription.TrialEnd?.ToUniversalTime(),
            currentPeriodEnd);
    }

    private static StripeSubscriptionEvent MapDeletedSubscription(Stripe.Event stripeEvent)
    {
        var subscription = GetObject<Stripe.Subscription>(stripeEvent);
        return new StripeSubscriptionEvent(
            Required(stripeEvent.Id, "Stripe event ID is required."),
            Required(stripeEvent.Type, "Stripe event type is required."),
            Required(subscription.Id, "Stripe subscription ID is required."));
    }

    private static StripeSubscriptionEvent MapPaidInvoice(Stripe.Event stripeEvent)
    {
        var invoice = GetObject<Stripe.Invoice>(stripeEvent);
        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
        if (invoice.PeriodEnd <= DateTime.UnixEpoch)
        {
            throw new StripeWebhookPayloadException("Stripe invoice period end is required.");
        }

        return new StripeSubscriptionEvent(
            Required(stripeEvent.Id, "Stripe event ID is required."),
            Required(stripeEvent.Type, "Stripe event type is required."),
            Required(subscriptionId, "Stripe invoice subscription ID is required."),
            CurrentPeriodEnd: invoice.PeriodEnd.ToUniversalTime());
    }

    private static T GetObject<T>(Stripe.Event stripeEvent)
        where T : class, Stripe.IHasObject
    {
        return stripeEvent.Data?.Object as T
            ?? throw new StripeWebhookPayloadException(
                $"Stripe event '{stripeEvent.Type}' does not contain the expected {typeof(T).Name} payload.");
    }

    private static Guid GetVenueId(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null ||
            !metadata.TryGetValue(VenueIdMetadataKey, out var value) ||
            !Guid.TryParse(value, out var venueId) ||
            venueId == Guid.Empty)
        {
            throw new StripeWebhookPayloadException(
                $"Stripe subscription metadata must contain a valid '{VenueIdMetadataKey}'.");
        }

        return venueId;
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

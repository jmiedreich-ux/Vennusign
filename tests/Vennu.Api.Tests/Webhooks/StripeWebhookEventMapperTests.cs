using Stripe;
using Vennu.Api.Webhooks;

namespace Vennu.Api.Tests.Webhooks;

[Trait("Category", "Unit")]
public sealed class StripeWebhookEventMapperTests
{
    [Fact]
    public void TryMap_MapsCreatedSubscription()
    {
        var venueId = Guid.NewGuid();
        var periodEnd = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var trialEnd = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Utc);
        var stripeEvent = CreateEvent(
            EventTypes.CustomerSubscriptionCreated,
            new Subscription
            {
                Id = "sub_123",
                Status = "trialing",
                CancelAtPeriodEnd = true,
                TrialEnd = trialEnd,
                Metadata = new Dictionary<string, string> { ["venue_id"] = venueId.ToString() },
                Items = new StripeList<SubscriptionItem>
                {
                    Data =
                    [
                        new SubscriptionItem
                        {
                            Price = new Price { Id = "price_pro_monthly" },
                            CurrentPeriodEnd = periodEnd
                        }
                    ]
                }
            });

        var mapped = StripeWebhookEventMapper.TryMap(stripeEvent, out var result);

        Assert.True(mapped);
        Assert.NotNull(result);
        Assert.Equal("evt_123", result.EventId);
        Assert.Equal(EventTypes.CustomerSubscriptionCreated, result.EventType);
        Assert.Equal("sub_123", result.StripeSubscriptionId);
        Assert.Equal(venueId, result.VenueId);
        Assert.Equal("price_pro_monthly", result.StripePriceId);
        Assert.Equal("trialing", result.Status);
        Assert.Equal(trialEnd, result.TrialEndsAt);
        Assert.Equal(periodEnd, result.CurrentPeriodEnd);
        Assert.True(result.CancelAtPeriodEnd);
    }

    [Fact]
    public void TryMap_PrefersOrganizationOwnershipAndMapsCustomer()
    {
        var organizationId = Guid.NewGuid();
        var stripeEvent = CreateEvent(
            EventTypes.CustomerSubscriptionCreated,
            new Subscription
            {
                Id = "sub_org",
                CustomerId = "cus_org",
                Status = "active",
                Metadata = new Dictionary<string, string> { ["organization_id"] = organizationId.ToString() },
                Items = new StripeList<SubscriptionItem>
                {
                    Data = [new SubscriptionItem { Price = new Price { Id = "price_pro_monthly" } }]
                }
            });

        Assert.True(StripeWebhookEventMapper.TryMap(stripeEvent, out var result));
        Assert.Equal(organizationId, result!.OrganizationId);
        Assert.Null(result.VenueId);
        Assert.Equal("cus_org", result.StripeCustomerId);
    }

    [Fact]
    public void TryMap_MapsPaidInvoice()
    {
        var periodEnd = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var stripeEvent = CreateEvent(
            EventTypes.InvoicePaid,
            new Invoice
            {
                Id = "in_123",
                Parent = new InvoiceParent
                {
                    SubscriptionDetails = new InvoiceParentSubscriptionDetails
                    {
                        SubscriptionId = "sub_123"
                    }
                },
                PeriodEnd = periodEnd
            });

        var mapped = StripeWebhookEventMapper.TryMap(stripeEvent, out var result);

        Assert.True(mapped);
        Assert.NotNull(result);
        Assert.Equal("sub_123", result.StripeSubscriptionId);
        Assert.Equal(periodEnd, result.CurrentPeriodEnd);
        Assert.Null(result.VenueId);
    }

    [Fact]
    public void TryMap_MapsDeletedSubscriptionWithoutMetadata()
    {
        var stripeEvent = CreateEvent(
            EventTypes.CustomerSubscriptionDeleted,
            new Subscription { Id = "sub_deleted" });

        var mapped = StripeWebhookEventMapper.TryMap(stripeEvent, out var result);

        Assert.True(mapped);
        Assert.NotNull(result);
        Assert.Equal("sub_deleted", result.StripeSubscriptionId);
        Assert.Null(result.VenueId);
    }

    [Fact]
    public void TryMap_ReturnsFalse_ForUnsupportedVerifiedEvent()
    {
        var stripeEvent = new Event { Id = "evt_unsupported", Type = EventTypes.CustomerCreated };

        var mapped = StripeWebhookEventMapper.TryMap(stripeEvent, out var result);

        Assert.False(mapped);
        Assert.Null(result);
    }

    [Fact]
    public void HaasMapper_MapsConfirmedBundleMetadata()
    {
        var venueId = Guid.NewGuid();
        var started = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc);
        var stripeEvent = CreateEvent(
            EventTypes.CustomerSubscriptionCreated,
            new Subscription
            {
                Id = "sub_haas",
                Status = "active",
                StartDate = started,
                Metadata = new Dictionary<string, string>
                {
                    ["venue_id"] = venueId.ToString(),
                    ["haas_bundle_key"] = "starter_kit",
                    ["haas_term_months"] = "18"
                }
            });

        var mapped = StripeHaasWebhookEventMapper.TryMap(stripeEvent, out var result);

        Assert.True(mapped);
        Assert.NotNull(result);
        Assert.Equal(venueId, result.VenueId);
        Assert.Equal("starter_kit", result.BundleKey);
        Assert.Equal(18, result.TermMonths);
        Assert.Equal(started, result.StartedUtc);
    }

    [Fact]
    public void TryMap_Throws_WhenSubscriptionVenueMetadataIsMissing()
    {
        var stripeEvent = CreateEvent(
            EventTypes.CustomerSubscriptionUpdated,
            new Subscription
            {
                Id = "sub_123",
                Status = "active",
                Metadata = new Dictionary<string, string>(),
                Items = new StripeList<SubscriptionItem>
                {
                    Data =
                    [
                        new SubscriptionItem
                        {
                            Price = new Price { Id = "price_pro_monthly" },
                            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
                        }
                    ]
                }
            });

        Assert.Throws<StripeWebhookPayloadException>(() =>
            StripeWebhookEventMapper.TryMap(stripeEvent, out _));
    }

    private static Event CreateEvent(string eventType, IHasObject payload) =>
        new()
        {
            Id = "evt_123",
            Type = eventType,
            Data = new EventData { Object = payload }
        };
}

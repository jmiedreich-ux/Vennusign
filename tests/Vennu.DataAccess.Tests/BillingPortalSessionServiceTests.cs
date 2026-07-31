using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class BillingPortalSessionServiceTests
{
    [Fact]
    public async Task CreateAsync_UsesClaimBoundVenueSubscription()
    {
        var venueId = Guid.NewGuid();
        var gateway = new GatewayFake();
        var service = new BillingPortalSessionService(
            new SubscriptionRepositoryFake(new VenueSubscription
            {
                VenueId = venueId,
                StripeSubscriptionId = " sub_123 ",
                Status = "active"
            }),
            gateway);

        var result = await service.CreateAsync(venueId);

        Assert.Equal("https://billing.stripe.com/p/session/test", result.PortalUrl.AbsoluteUri);
        Assert.Equal("sub_123", Assert.Single(gateway.Requests).StripeSubscriptionId);
    }

    [Fact]
    public async Task CreateAsync_RejectsVenueWithoutSubscription()
    {
        var service = new BillingPortalSessionService(
            new SubscriptionRepositoryFake(null),
            new GatewayFake());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(Guid.NewGuid()));
    }

    [Theory]
    [InlineData("", "active")]
    [InlineData("sub_123", "canceled")]
    public async Task CreateAsync_RejectsSubscriptionThatCannotBeManaged(string stripeId, string status)
    {
        var venueId = Guid.NewGuid();
        var service = new BillingPortalSessionService(
            new SubscriptionRepositoryFake(new VenueSubscription
            {
                VenueId = venueId,
                StripeSubscriptionId = stripeId,
                Status = status
            }),
            new GatewayFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(venueId));
    }

    private sealed class GatewayFake : IStripeBillingPortalSessionGateway
    {
        public List<StripeBillingPortalSessionRequest> Requests { get; } = [];

        public Task<StripeBillingPortalSessionResult> CreateAsync(
            StripeBillingPortalSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(new StripeBillingPortalSessionResult(
                new Uri("https://billing.stripe.com/p/session/test")));
        }
    }

    private sealed class SubscriptionRepositoryFake(VenueSubscription? subscription)
        : IVenueSubscriptionRepository
    {
        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VenueSubscription>>(
                subscription is null ? [] : [subscription]);

        public Task<VenueSubscription?> GetByVenueIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(subscription?.VenueId == venueId ? subscription : null);

        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(
            string stripeSubscriptionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                string.Equals(subscription?.StripeSubscriptionId, stripeSubscriptionId, StringComparison.Ordinal)
                    ? subscription
                    : null);

        public Task<bool> SaveAsync(
            VenueSubscription value,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }
}

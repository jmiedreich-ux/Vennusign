using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public sealed class CheckoutSessionServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TierId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Theory]
    [InlineData(CheckoutBillingInterval.Monthly, "price_pro_month")]
    [InlineData(CheckoutBillingInterval.Annual, "price_pro_year")]
    public async Task CreateAsync_UsesMappedPrice(
        CheckoutBillingInterval interval,
        string expectedPrice)
    {
        var gateway = new GatewayFake();
        var service = CreateService(PublicTier(), gateway);

        var result = await service.CreateAsync(VenueId, TierId, interval);

        Assert.Equal("https://checkout.stripe.com/c/pay/test", result.CheckoutUrl.AbsoluteUri);
        Assert.Equal(expectedPrice, gateway.LastRequest?.PriceId);
        Assert.Equal(VenueId, gateway.LastRequest?.VenueId);
        Assert.Equal("pro", gateway.LastRequest?.TierSlug);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task CreateAsync_RejectsUnavailableTier(bool isActive, bool isPublic)
    {
        var tier = PublicTier();
        tier.IsActive = isActive;
        tier.IsPublic = isPublic;
        var gateway = new GatewayFake();
        var service = CreateService(tier, gateway);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(VenueId, TierId, CheckoutBillingInterval.Monthly));

        Assert.Null(gateway.LastRequest);
    }

    [Fact]
    public async Task CreateAsync_RejectsMissingPriceMapping()
    {
        var tier = PublicTier();
        tier.StripeAnnualPriceId = null;
        var gateway = new GatewayFake();
        var service = CreateService(tier, gateway);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(VenueId, TierId, CheckoutBillingInterval.Annual));

        Assert.Null(gateway.LastRequest);
    }

    [Fact]
    public async Task CreateAsync_RejectsVenueWithoutSubscription()
    {
        var gateway = new GatewayFake();
        var service = new CheckoutSessionService(
            new BillingCatalogRepositoryFake(PublicTier()),
            new SubscriptionRepositoryFake(null),
            gateway);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(VenueId, TierId, CheckoutBillingInterval.Monthly));

        Assert.Null(gateway.LastRequest);
    }

    [Fact]
    public async Task CreateForOrganization_UsesOrganizationOwnershipAndCustomer()
    {
        var organizationId = Guid.NewGuid();
        var gateway = new GatewayFake();
        var service = new CheckoutSessionService(
            new BillingCatalogRepositoryFake(PublicTier()),
            new SubscriptionRepositoryFake(null),
            gateway,
            null,
            new OrganizationSubscriptionRepositoryFake(new OrganizationSubscription
            {
                OrganizationId = organizationId,
                TierId = TierId,
                StripeCustomerId = "cus_org",
                Status = "trialing"
            }));

        await service.CreateForOrganizationAsync(organizationId, TierId, CheckoutBillingInterval.Monthly);

        Assert.Equal(organizationId, gateway.LastRequest!.OrganizationId);
        Assert.Equal(Guid.Empty, gateway.LastRequest.VenueId);
        Assert.Equal("cus_org", gateway.LastRequest.StripeCustomerId);
    }

    [Fact]
    public async Task CreateForOrganization_AllowsFirstPaidCheckoutWithoutExistingSubscription()
    {
        var organizationId = Guid.NewGuid();
        var gateway = new GatewayFake();
        var service = new CheckoutSessionService(
            new BillingCatalogRepositoryFake(PublicTier()),
            new SubscriptionRepositoryFake(null),
            gateway,
            null,
            new OrganizationSubscriptionRepositoryFake(null));

        await service.CreateForOrganizationAsync(organizationId, TierId, CheckoutBillingInterval.Annual);

        Assert.Equal(organizationId, gateway.LastRequest!.OrganizationId);
        Assert.Null(gateway.LastRequest.StripeCustomerId);
        Assert.Equal("price_pro_year", gateway.LastRequest.PriceId);
    }

    private static CheckoutSessionService CreateService(
        SubscriptionTier tier,
        GatewayFake gateway) =>
        new(
            new BillingCatalogRepositoryFake(tier),
            new SubscriptionRepositoryFake(new VenueSubscription
            {
                VenueId = VenueId,
                TierId = Guid.NewGuid()
            }),
            gateway);

    private static SubscriptionTier PublicTier() =>
        new()
        {
            Id = TierId,
            Name = "Pro",
            Slug = "pro",
            IsActive = true,
            IsPublic = true,
            StripeMonthlyPriceId = "price_pro_month",
            StripeAnnualPriceId = "price_pro_year"
        };

    private sealed class GatewayFake : IStripeCheckoutSessionGateway
    {
        public StripeCheckoutSessionRequest? LastRequest { get; private set; }

        public Task<StripeCheckoutSessionResult> CreateAsync(
            StripeCheckoutSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(
                new StripeCheckoutSessionResult(
                    new Uri("https://checkout.stripe.com/c/pay/test")));
        }
    }

    private sealed class BillingCatalogRepositoryFake(SubscriptionTier tier)
        : IBillingCatalogRepository
    {
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<SubscriptionTier>>([tier]);

        public Task<SubscriptionTier?> GetByIdAsync(
            Guid tierId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<SubscriptionTier?>(tier.Id == tierId ? tier : null);

        public Task<SubscriptionTier?> GetByStripeProductIdAsync(
            string productId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<SubscriptionTier?>(null);

        public Task<SubscriptionTier?> GetByStripePriceIdAsync(
            string priceId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<SubscriptionTier?>(null);

        public Task<bool> SaveAsync(
            SubscriptionTier value,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
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
            Task.FromResult<VenueSubscription?>(null);

        public Task<bool> SaveAsync(
            VenueSubscription value,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class OrganizationSubscriptionRepositoryFake(OrganizationSubscription? subscription)
        : IOrganizationSubscriptionRepository
    {
        public Task<IReadOnlyCollection<OrganizationSubscription>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<OrganizationSubscription>>(subscription is null ? [] : [subscription]);
        public Task<OrganizationSubscription?> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
            Task.FromResult<OrganizationSubscription?>(subscription?.OrganizationId == organizationId ? subscription : null);
        public Task<OrganizationSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<OrganizationSubscription?>(subscription?.StripeSubscriptionId == stripeSubscriptionId ? subscription : null);
        public Task<bool> SaveAsync(OrganizationSubscription value, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}

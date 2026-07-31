using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class HaasBillingServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 31, 18, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("starter_kit", 18, 89)]
    [InlineData("bar_pack", 24, 159)]
    [InlineData("full_house", 36, 249)]
    public async Task CreateCheckoutAsync_AcceptsOnlyApprovedBundleTerm(
        string bundleKey,
        int termMonths,
        decimal monthlyAmount)
    {
        var gateway = new GatewayFake();
        var service = CreateService(gateway: gateway);

        var result = await service.CreateCheckoutAsync(VenueId, bundleKey, termMonths);

        Assert.Equal("https://checkout.stripe.com/c/pay/haas", result.CheckoutUrl.AbsoluteUri);
        var request = Assert.Single(gateway.Requests);
        Assert.Equal(VenueId, request.VenueId);
        Assert.Equal(bundleKey, request.BundleKey);
        Assert.Equal(termMonths, request.TermMonths);
        Assert.Equal(monthlyAmount, request.MonthlyAmount);
    }

    [Theory]
    [InlineData("starter_kit", 24)]
    [InlineData("bar_pack", 18)]
    [InlineData("unknown", 36)]
    public async Task CreateCheckoutAsync_RejectsUnapprovedBundleTerm(string bundleKey, int termMonths)
    {
        var gateway = new GatewayFake();
        var service = CreateService(gateway: gateway);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateCheckoutAsync(VenueId, bundleKey, termMonths));

        Assert.Empty(gateway.Requests);
    }

    [Fact]
    public async Task Presentation_DisclosesDeterministicRemainingTermWithoutCollecting()
    {
        var contract = new HaasContract
        {
            VenueId = VenueId,
            BundleKey = "starter_kit",
            TermMonths = 18,
            MonthlyAmount = 89m,
            StripeSubscriptionId = "sub_haas",
            Status = "active",
            StartedUtc = UtcNow.UtcDateTime.AddMonths(-6),
            ContractEndsUtc = UtcNow.UtcDateTime.AddMonths(12)
        };
        var service = CreateService(contract);

        var presentation = await service.GetPresentationAsync(VenueId);

        Assert.Equal(3, presentation.Bundles.Count);
        Assert.NotNull(presentation.Contract);
        Assert.Equal(12, presentation.Contract.RemainingMonths);
        Assert.Equal(1068m, presentation.Contract.EstimatedBuyoutAmount);
    }

    private static HaasBillingService CreateService(
        HaasContract? contract = null,
        GatewayFake? gateway = null) =>
        new(
            new SubscriptionRepositoryFake(new VenueSubscription { VenueId = VenueId }),
            new ContractRepositoryFake(contract),
            gateway ?? new GatewayFake(),
            new FixedTimeProvider(UtcNow));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class GatewayFake : IStripeHaasCheckoutSessionGateway
    {
        public List<StripeHaasCheckoutSessionRequest> Requests { get; } = [];

        public Task<StripeHaasCheckoutSessionResult> CreateAsync(
            StripeHaasCheckoutSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(new StripeHaasCheckoutSessionResult(
                new Uri("https://checkout.stripe.com/c/pay/haas")));
        }
    }

    private sealed class ContractRepositoryFake(HaasContract? contract) : IHaasContractRepository
    {
        public Task<HaasContract?> GetCurrentByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(contract?.VenueId == venueId ? contract : null);

        public Task<HaasContract?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(contract?.StripeSubscriptionId == stripeSubscriptionId ? contract : null);

        public Task<bool> SaveAsync(HaasContract value, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class SubscriptionRepositoryFake(VenueSubscription? subscription) : IVenueSubscriptionRepository
    {
        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VenueSubscription>>(subscription is null ? [] : [subscription]);
        public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(subscription?.VenueId == venueId ? subscription : null);
        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<VenueSubscription?>(null);
        public Task<bool> SaveAsync(VenueSubscription value, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}

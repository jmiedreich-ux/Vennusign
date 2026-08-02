using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class CustomerOnboardingServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 2, 7, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task PublicPlans_ExposeOnlyActivePublicPolicyWithoutStripeIds()
    {
        var publicTier = Tier(trialDays: 14);
        var service = Create(tiers: new TierFake(publicTier, Tier(isPublic: false), Tier(isActive: false)));

        var plan = Assert.Single(await service.GetPublicPlansAsync());

        Assert.Equal(publicTier.Id, plan.Id);
        Assert.Equal(14, plan.TrialDays);
        Assert.True(plan.MonthlyCheckoutAvailable);
    }

    [Fact]
    public async Task CreateOrganization_AndTrial_PersistOneOwnedResumableJourney()
    {
        var userId = Guid.NewGuid();
        var tier = Tier(trialDays: 21);
        var states = new OnboardingFake();
        var subscriptions = new OrganizationSubscriptionsFake();
        var service = Create(states, new TierFake(tier), subscriptions);

        var organization = await service.CreateOrganizationAsync(userId, "My Bar");
        var trial = await service.StartTrialAsync(userId, tier.Id);

        Assert.NotNull(organization.OrganizationId);
        Assert.Equal("venue", trial.CurrentStep);
        Assert.Equal("trialing", trial.EntitlementStatus);
        Assert.Equal(Now.UtcDateTime.AddDays(21), trial.TrialEndsAt);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateOrganizationAsync(userId, "Another"));
    }

    [Fact]
    public async Task Checkout_PersistsSelectionButDoesNotGrantEntitlement()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var tier = Tier();
        var states = new OnboardingFake(new CustomerOnboardingState
        {
            UserId = userId, OrganizationId = organizationId, CreatedUtc = Now.UtcDateTime, UpdatedUtc = Now.UtcDateTime
        });
        var checkout = new CheckoutFake();
        var service = Create(states, new TierFake(tier), checkout: checkout);

        var result = await service.CreateCheckoutAsync(userId, tier.Id, CheckoutBillingInterval.Monthly);
        var snapshot = await service.GetAsync(userId);

        Assert.Equal("https://checkout.stripe.com/c/pay", result.CheckoutUrl.AbsoluteUri);
        Assert.Equal(tier.Id, states.State!.SelectedTierId);
        Assert.True(snapshot.CheckoutPending);
        Assert.False(snapshot.Progress.Plan);
        Assert.Equal(organizationId, checkout.OrganizationId);
    }

    private static CustomerOnboardingService Create(
        OnboardingFake? states = null,
        TierFake? tiers = null,
        OrganizationSubscriptionsFake? subscriptions = null,
        CheckoutFake? checkout = null)
    {
        states ??= new OnboardingFake();
        tiers ??= new TierFake(Tier());
        subscriptions ??= new OrganizationSubscriptionsFake();
        return new CustomerOnboardingService(
            states,
            tiers,
            subscriptions,
            new MembershipFake(),
            new SubscriptionManagementFake(subscriptions, tiers),
            checkout ?? new CheckoutFake(),
            new FixedTimeProvider());
    }

    private static SubscriptionTier Tier(bool isPublic = true, bool isActive = true, int trialDays = 14) => new()
    {
        Id = Guid.NewGuid(), Name = "Starter", Slug = Guid.NewGuid().ToString("N"), Price = 29,
        MaxVenues = 1, MaxScreens = 3, TrialDays = trialDays, IsPublic = isPublic, IsActive = isActive,
        StripeMonthlyPriceId = "price_monthly"
    };

    private sealed class FixedTimeProvider : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }

    private sealed class OnboardingFake(params CustomerOnboardingState[] states) : ICustomerOnboardingRepository
    {
        public CustomerOnboardingState? State { get; private set; } = states.SingleOrDefault();
        public Task<CustomerOnboardingState?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(State?.UserId == userId ? State : null);
        public Task<CustomerOnboardingState> SaveAsync(CustomerOnboardingState state, CancellationToken cancellationToken = default)
        { State = state; return Task.FromResult(state); }
    }

    private sealed class TierFake(params SubscriptionTier[] values) : ISubscriptionTierRepository
    {
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<SubscriptionTier>>(values);
        public Task<SubscriptionTier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(values.SingleOrDefault(value => value.Id == id));
        public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.FromResult(values.SingleOrDefault(value => value.Slug == slug));
        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
        public Task<bool> CreateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> UpdateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class OrganizationSubscriptionsFake : IOrganizationSubscriptionRepository
    {
        public OrganizationSubscription? Value { get; set; }
        public Task<IReadOnlyCollection<OrganizationSubscription>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<OrganizationSubscription>>(Value is null ? [] : [Value]);
        public Task<OrganizationSubscription?> GetByOrganizationIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Value?.OrganizationId == id ? Value : null);
        public Task<OrganizationSubscription?> GetByStripeSubscriptionIdAsync(string id, CancellationToken cancellationToken = default) => Task.FromResult(Value?.StripeSubscriptionId == id ? Value : null);
        public Task<bool> SaveAsync(OrganizationSubscription value, CancellationToken cancellationToken = default) { Value = value; return Task.FromResult(true); }
    }

    private sealed class MembershipFake : IIdentityMembershipService
    {
        public Task<Organization> CreateOrganizationAsync(string name, Guid ownerUserId, CancellationToken cancellationToken = default) => Task.FromResult(new Organization { Id = Guid.NewGuid(), Name = name, OwnerUserId = ownerUserId, CreatedUtc = Now.UtcDateTime, UpdatedUtc = Now.UtcDateTime });
        public Task<OrganizationMembership> AddOrChangeOrganizationMemberAsync(Guid a, Guid b, Guid c, OrganizationMembershipRole d, CancellationToken e = default) => throw new NotSupportedException();
        public Task RevokeOrganizationMemberAsync(Guid a, Guid b, Guid c, CancellationToken d = default) => throw new NotSupportedException();
        public Task TransferOwnershipAsync(Guid a, Guid b, Guid c, CancellationToken d = default) => throw new NotSupportedException();
        public Task AttachVenueAsync(Guid a, Guid b, Guid c, CancellationToken d = default) => throw new NotSupportedException();
        public Task<VenueMembership> AddOrChangeVenueMemberAsync(Guid a, Guid b, Guid c, Guid d, VenueMembershipRole e, CancellationToken f = default) => throw new NotSupportedException();
        public Task RevokeVenueMemberAsync(Guid a, Guid b, Guid c, Guid d, CancellationToken e = default) => throw new NotSupportedException();
    }

    private sealed class SubscriptionManagementFake(OrganizationSubscriptionsFake subscriptions, TierFake tiers) : IOrganizationSubscriptionManagementService
    {
        public async Task<OrganizationSubscription> StartTrialAsync(Guid organizationId, Guid tierId, CancellationToken cancellationToken = default)
        {
            var tier = await tiers.GetByIdAsync(tierId, cancellationToken) ?? throw new InvalidOperationException();
            var value = new OrganizationSubscription { OrganizationId = organizationId, TierId = tierId, Status = "trialing", TrialEndsAt = Now.UtcDateTime.AddDays(tier.TrialDays) };
            await subscriptions.SaveAsync(value, cancellationToken); return value;
        }
        public Task EnsureCanAddVenueAsync(Guid organizationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> ExpireTrialsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class CheckoutFake : ICheckoutSessionService
    {
        public Guid OrganizationId { get; private set; }
        public Task<StripeCheckoutSessionResult> CreateForOrganizationAsync(Guid organizationId, Guid tierId, CheckoutBillingInterval interval, CancellationToken cancellationToken = default)
        { OrganizationId = organizationId; return Task.FromResult(new StripeCheckoutSessionResult(new Uri("https://checkout.stripe.com/c/pay"))); }
        public Task<StripeCheckoutSessionResult> CreateAsync(Guid venueId, Guid tierId, CheckoutBillingInterval interval, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}

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

        var organization = await service.CreateOrganizationAsync(userId,
            new OrganizationProfile("My Bar", "My Bar LLC", "Alex Owner", "owner@example.com", null, "1 Main St, New York, NY 10001"));
        var trial = await service.StartTrialAsync(userId, tier.Id);

        Assert.NotNull(organization.OrganizationId);
        Assert.Equal("owner@example.com", organization.Organization?.ContactEmail);
        Assert.Equal("venue", trial.CurrentStep);
        Assert.Equal("trialing", trial.EntitlementStatus);
        Assert.Equal(Now.UtcDateTime.AddDays(21), trial.TrialEndsAt);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateOrganizationAsync(userId,
            new OrganizationProfile("Another", null, "Alex", "alex@example.com", null, "2 Main St")));
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

    [Fact]
    public async Task CreateVenue_PersistsOrganizationOwnedEntitledVenue()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tier = Tier();
        var states = new OnboardingFake(new CustomerOnboardingState
        {
            UserId = userId, OrganizationId = organizationId, SelectedTierId = tier.Id,
            CreatedUtc = Now.UtcDateTime, UpdatedUtc = Now.UtcDateTime
        });
        var subscriptions = new OrganizationSubscriptionsFake
        {
            Value = new OrganizationSubscription { OrganizationId = organizationId, TierId = tier.Id, Status = "active" }
        };
        var venueProvisioning = new VenueProvisioningFake(venueId);
        var service = Create(states, new TierFake(tier), subscriptions, venueProvisioning: venueProvisioning);

        var snapshot = await service.CreateVenueAsync(userId, new CustomerOnboardingVenueRequest(
            "  My Venue  ", "America/New_York", "restaurant", "EN", "es"));

        Assert.Equal(venueId, snapshot.VenueId);
        Assert.Equal("first-screen", snapshot.CurrentStep);
        Assert.Equal(organizationId, venueProvisioning.Venue!.OrganizationId);
        Assert.Equal("My Venue", venueProvisioning.Venue.Name);
        Assert.Equal("en", venueProvisioning.Venue.PrimaryLanguage);
    }

    [Fact]
    public async Task ClaimFirstScreen_LinksDeviceButRequiresOnlineHeartbeatForGoLive()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var tier = Tier();
        var states = new OnboardingFake(new CustomerOnboardingState
        {
            UserId = userId, OrganizationId = organizationId, VenueId = venueId,
            CreatedUtc = Now.UtcDateTime, UpdatedUtc = Now.UtcDateTime
        });
        var subscriptions = new OrganizationSubscriptionsFake
        {
            Value = new OrganizationSubscription { OrganizationId = organizationId, TierId = tier.Id, Status = "active" }
        };
        var screens = new ScreensFake(new Screen { Id = screenId, ScreenKey = "sc-test01", Name = "Display", Status = "Offline" });
        var pairing = new PairingFake(new ScreenPairingCode { Code = "123456", ScreenId = screenId, ExpiresAt = Now.UtcDateTime.AddMinutes(5) });
        var service = Create(states, new TierFake(tier), subscriptions, pairing: pairing, screens: screens);

        var paired = await service.ClaimFirstScreenAsync(userId, "123456");

        Assert.Equal(screenId, paired.FirstScreenId);
        Assert.Equal("paired-offline", paired.FirstScreenStatus);
        Assert.True(paired.Progress.FirstScreen);
        Assert.False(paired.Progress.GoLive);
        Assert.Equal(venueId, screens.Screen!.VenueId);

        screens.Screen.Status = "Online";
        screens.Screen.LastSeen = Now.UtcDateTime;
        var online = await service.GetAsync(userId);
        Assert.Equal("online", online.FirstScreenStatus);
        Assert.True(online.Progress.GoLive);
    }

    [Fact]
    public async Task ClaimFirstScreen_ExpiredCodeDoesNotAdvanceState()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var tier = Tier();
        var states = new OnboardingFake(new CustomerOnboardingState
        {
            UserId = userId, OrganizationId = organizationId, VenueId = venueId,
            CreatedUtc = Now.UtcDateTime, UpdatedUtc = Now.UtcDateTime
        });
        var subscriptions = new OrganizationSubscriptionsFake
        {
            Value = new OrganizationSubscription { OrganizationId = organizationId, TierId = tier.Id, Status = "active" }
        };
        var pairing = new PairingFake(new ScreenPairingCode { Code = "123456", ScreenId = Guid.NewGuid(), ExpiresAt = Now.UtcDateTime.AddSeconds(-1) });
        var service = Create(states, new TierFake(tier), subscriptions, pairing: pairing);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClaimFirstScreenAsync(userId, "123456"));
        Assert.Null(states.State!.FirstScreenId);
        Assert.False(pairing.Claimed);
    }

    private static CustomerOnboardingService Create(
        OnboardingFake? states = null,
        TierFake? tiers = null,
        OrganizationSubscriptionsFake? subscriptions = null,
        CheckoutFake? checkout = null,
        VenueProvisioningFake? venueProvisioning = null,
        VenueEntitlementFake? venueEntitlement = null,
        PairingFake? pairing = null,
        ScreensFake? screens = null)
    {
        states ??= new OnboardingFake();
        tiers ??= new TierFake(Tier());
        subscriptions ??= new OrganizationSubscriptionsFake();
        var organizations = new OrganizationRepositoryFake();
        return new CustomerOnboardingService(
            states,
            tiers,
            subscriptions,
            organizations,
            new MembershipFake(organizations),
            new SubscriptionManagementFake(subscriptions, tiers),
            checkout ?? new CheckoutFake(),
            venueProvisioning ?? new VenueProvisioningFake(Guid.NewGuid()),
            venueEntitlement ?? new VenueEntitlementFake(),
            pairing ?? new PairingFake(),
            screens ?? new ScreensFake(),
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

    private sealed class OrganizationRepositoryFake : IOrganizationMembershipRepository
    {
        public Organization? Value { get; set; }
        public Task<Organization?> GetOrganizationAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Value?.Id == id ? Value : null);
        public Task<OrganizationMembership?> GetOrganizationMembershipAsync(Guid a, Guid b, CancellationToken c = default) => throw new NotSupportedException();
        public Task<VenueMembership?> GetVenueMembershipAsync(Guid a, Guid b, Guid c, CancellationToken d = default) => throw new NotSupportedException();
        public Task<Organization> CreateOrganizationAsync(Organization a, OrganizationMembership b, MembershipAuditEntry c, CancellationToken d = default) => throw new NotSupportedException();
        public Task<OrganizationMembership> SaveOrganizationMembershipAsync(OrganizationMembership a, MembershipAuditEntry b, CancellationToken c = default) => throw new NotSupportedException();
        public Task TransferOwnershipAsync(Guid a, Guid b, Guid c, DateTime d, MembershipAuditEntry e, CancellationToken f = default) => throw new NotSupportedException();
        public Task AttachVenueAsync(Guid a, Guid b, MembershipAuditEntry c, CancellationToken d = default) => throw new NotSupportedException();
        public Task<VenueMembership> SaveVenueMembershipAsync(VenueMembership a, MembershipAuditEntry b, CancellationToken c = default) => throw new NotSupportedException();
    }

    private sealed class MembershipFake(OrganizationRepositoryFake organizations) : IIdentityMembershipService
    {
        public Task<Organization> CreateOrganizationAsync(string name, Guid ownerUserId, CancellationToken cancellationToken = default) => CreateOrganizationAsync(new OrganizationProfile(name, null, "", "", null, ""), ownerUserId, cancellationToken);
        public Task<Organization> CreateOrganizationAsync(OrganizationProfile profile, Guid ownerUserId, CancellationToken cancellationToken = default)
        {
            organizations.Value = new Organization { Id = Guid.NewGuid(), Name = profile.Name, LegalName = profile.LegalName, PrimaryContactName = profile.PrimaryContactName, ContactEmail = profile.ContactEmail, ContactPhone = profile.ContactPhone, MailingAddress = profile.MailingAddress, OwnerUserId = ownerUserId, CreatedUtc = Now.UtcDateTime, UpdatedUtc = Now.UtcDateTime };
            return Task.FromResult(organizations.Value);
        }
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

    private sealed class VenueProvisioningFake(Guid venueId) : IVenueProvisioningService
    {
        public Venue? Venue { get; private set; }
        public Task<VenueProvisioningResult> ProvisionAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            Venue = venue;
            return Task.FromResult(new VenueProvisioningResult(venueId, new VenueSubscription { VenueId = venueId, TierId = Guid.NewGuid() }));
        }
    }

    private sealed class VenueEntitlementFake : IVenueEntitlementService
    {
        public Task EnsureCanAddScreenAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task EnsureCanAddVenueAsync(Guid organizationId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class PairingFake(params ScreenPairingCode[] values) : IScreenPairingCodeRepository
    {
        public bool Claimed { get; private set; }
        public Task<string> CreateAsync(ScreenPairingCode value, CancellationToken cancellationToken = default) => Task.FromResult(value.Code);
        public Task<ScreenPairingCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(values.SingleOrDefault(value => value.Code == code));
        public Task<bool> ClaimAsync(string code, Guid venueId, CancellationToken cancellationToken = default)
        {
            var value = values.SingleOrDefault(item => item.Code == code);
            if (value is null || value.IsClaimed) return Task.FromResult(false);
            value.IsClaimed = true; value.VenueId = venueId; Claimed = true; return Task.FromResult(true);
        }
    }

    private sealed class ScreensFake(params Screen[] values) : IScreenRepository
    {
        public Screen? Screen { get; private set; } = values.SingleOrDefault();
        public Task<Guid> CreateAsync(Screen screen, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> AssignVenueAsync(Guid screenId, Guid venueId, CancellationToken cancellationToken = default)
        {
            if (Screen?.Id != screenId) return Task.FromResult(false);
            Screen.VenueId = venueId; return Task.FromResult(true);
        }
        public Task<Screen?> GetByIdAsync(Guid screenId, CancellationToken cancellationToken = default) => Task.FromResult(Screen?.Id == screenId ? Screen : null);
        public Task<Screen?> GetByScreenKeyAsync(string screenKey, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Screen?> GetByPreRegistrationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<Screen>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Screen>>(Screen is null ? [] : [Screen]);
        public Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Screen>>(Screen is { VenueId: var currentVenueId } screen && currentVenueId == venueId ? [screen] : []);
        public Task<bool> UpdateAsync(Screen screen, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> ClaimPreRegisteredAsync(Guid screenId, string platform, string appVersion, DateTime claimedUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, string? platform, string? appVersion, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> MarkStaleOnlineScreensOfflineAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}

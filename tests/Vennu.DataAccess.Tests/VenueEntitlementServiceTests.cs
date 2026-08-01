using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class VenueEntitlementServiceTests
{
    [Fact]
    public async Task EnsureCanAddScreen_RejectsExpiredTrialAndTierLimit()
    {
        var venueId = Guid.NewGuid(); var tierId = Guid.NewGuid();
        var subscription = new VenueSubscription { VenueId=venueId, TierId=tierId, Status="trialing", TrialEndsAt=DateTime.UtcNow.AddMinutes(-1) };
        var service = new VenueEntitlementService(new SubscriptionFake(subscription), new TierFake(new SubscriptionTier { Id=tierId, MaxScreens=1 }), new ScreenFake([]), TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EnsureCanAddScreenAsync(venueId));

        subscription.TrialEndsAt = DateTime.UtcNow.AddDays(1);
        service = new VenueEntitlementService(new SubscriptionFake(subscription), new TierFake(new SubscriptionTier { Id=tierId, MaxScreens=1 }), new ScreenFake([new Screen { Id=Guid.NewGuid(), VenueId=venueId }]), TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EnsureCanAddScreenAsync(venueId));
    }

    private sealed class SubscriptionFake(VenueSubscription value) : IVenueSubscriptionRepository
    {
        public Task<VenueSubscription?> GetByVenueIdAsync(Guid id,CancellationToken ct=default)=>Task.FromResult<VenueSubscription?>(value);
        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(string id,CancellationToken ct=default)=>Task.FromResult<VenueSubscription?>(null);
        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken ct=default)=>Task.FromResult<IReadOnlyCollection<VenueSubscription>>([value]);
        public Task<bool> SaveAsync(VenueSubscription s,CancellationToken ct=default)=>Task.FromResult(true);
    }
    private sealed class TierFake(SubscriptionTier value) : ISubscriptionTierRepository
    {
        public Task<SubscriptionTier?> GetByIdAsync(Guid id,CancellationToken ct=default)=>Task.FromResult<SubscriptionTier?>(value);
        public Task<SubscriptionTier?> GetBySlugAsync(string s,CancellationToken ct=default)=>Task.FromResult<SubscriptionTier?>(null);
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken ct=default)=>Task.FromResult<IReadOnlyCollection<SubscriptionTier>>([value]);
        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid id,CancellationToken ct=default)=>Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
        public Task<bool>CreateAsync(SubscriptionTier t,CancellationToken ct=default)=>Task.FromResult(false); public Task<bool>UpdateAsync(SubscriptionTier t,CancellationToken ct=default)=>Task.FromResult(false);
    }
    private sealed class ScreenFake(IReadOnlyCollection<Screen> values) : IScreenRepository
    {
        public Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid id,CancellationToken ct=default)=>Task.FromResult(values);
        public Task<Guid>CreateAsync(Screen s,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<bool>AssignVenueAsync(Guid a,Guid b,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<Screen?>GetByIdAsync(Guid a,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<Screen?>GetByScreenKeyAsync(string a,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<Screen?>GetByPreRegistrationTokenHashAsync(string a,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<IReadOnlyCollection<Screen>>GetAllAsync(CancellationToken ct=default)=>throw new NotSupportedException(); public Task<bool>UpdateAsync(Screen s,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<bool>ClaimPreRegisteredAsync(Guid a,string b,string c,DateTime d,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<bool>UpdateHeartbeatAsync(Guid a,DateTime b,string c,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<bool>UpdateHeartbeatAsync(Guid a,DateTime b,string c,string? d,string? e,CancellationToken ct=default)=>throw new NotSupportedException(); public Task<int>MarkStaleOnlineScreensOfflineAsync(DateTime a,CancellationToken ct=default)=>throw new NotSupportedException();
    }
}

using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.TestDoubles;

internal sealed class FakeVenueRepository : IVenueRepository
{
    public Func<Venue, CancellationToken, Task<Guid>>? CreateAsyncHandler { get; set; }
    public Func<Guid, CancellationToken, Task<Venue?>>? GetByIdAsyncHandler { get; set; }
    public Func<CancellationToken, Task<IReadOnlyCollection<Venue>>>? GetAllAsyncHandler { get; set; }
    public Venue? LastCreatedVenue { get; private set; }

    public Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        LastCreatedVenue = venue;
        if (CreateAsyncHandler is not null) return CreateAsyncHandler(venue, cancellationToken);
        venue.Id = venue.Id == Guid.Empty ? Guid.NewGuid() : venue.Id;
        return Task.FromResult(venue.Id);
    }

    public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsyncHandler is not null ? GetAllAsyncHandler(cancellationToken) : Task.FromResult<IReadOnlyCollection<Venue>>([]);

    public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        GetByIdAsyncHandler is not null ? GetByIdAsyncHandler(venueId, cancellationToken) : Task.FromResult<Venue?>(null);
}

internal sealed class FakeScreenRepository : IScreenRepository
{
    public Func<Screen, CancellationToken, Task<Guid>>? CreateAsyncHandler { get; set; }
    public Func<Guid, Guid, CancellationToken, Task<bool>>? AssignVenueAsyncHandler { get; set; }
    public Func<Guid, CancellationToken, Task<Screen?>>? GetByIdAsyncHandler { get; set; }
    public Func<string, CancellationToken, Task<Screen?>>? GetByScreenKeyAsyncHandler { get; set; }
    public Func<CancellationToken, Task<IReadOnlyCollection<Screen>>>? GetAllAsyncHandler { get; set; }
    public Func<Guid, CancellationToken, Task<IReadOnlyCollection<Screen>>>? GetByVenueIdAsyncHandler { get; set; }
    public Func<Screen, CancellationToken, Task<bool>>? UpdateAsyncHandler { get; set; }
    public Func<Guid, DateTime, string, CancellationToken, Task<bool>>? UpdateHeartbeatAsyncHandler { get; set; }
    public Func<DateTime, CancellationToken, Task<int>>? MarkStaleOnlineScreensOfflineAsyncHandler { get; set; }
    public Screen? LastCreatedScreen { get; private set; }
    public Screen? LastUpdatedScreen { get; private set; }

    public Task<Guid> CreateAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        LastCreatedScreen = screen;
        if (CreateAsyncHandler is not null) return CreateAsyncHandler(screen, cancellationToken);
        screen.Id = screen.Id == Guid.Empty ? Guid.NewGuid() : screen.Id;
        return Task.FromResult(screen.Id);
    }

    public Task<bool> AssignVenueAsync(Guid screenId, Guid venueId, CancellationToken cancellationToken = default) =>
        AssignVenueAsyncHandler is not null ? AssignVenueAsyncHandler(screenId, venueId, cancellationToken) : Task.FromResult(true);

    public Task<Screen?> GetByIdAsync(Guid screenId, CancellationToken cancellationToken = default) =>
        GetByIdAsyncHandler is not null ? GetByIdAsyncHandler(screenId, cancellationToken) : Task.FromResult<Screen?>(null);

    public Task<Screen?> GetByScreenKeyAsync(string screenKey, CancellationToken cancellationToken = default) =>
        GetByScreenKeyAsyncHandler is not null ? GetByScreenKeyAsyncHandler(screenKey, cancellationToken) : Task.FromResult<Screen?>(null);

    public Task<IReadOnlyCollection<Screen>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsyncHandler is not null ? GetAllAsyncHandler(cancellationToken) : Task.FromResult<IReadOnlyCollection<Screen>>([]);

    public Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        GetByVenueIdAsyncHandler is not null ? GetByVenueIdAsyncHandler(venueId, cancellationToken) : Task.FromResult<IReadOnlyCollection<Screen>>([]);

    public Task<bool> UpdateAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        LastUpdatedScreen = screen;
        return UpdateAsyncHandler is not null ? UpdateAsyncHandler(screen, cancellationToken) : Task.FromResult(true);
    }

    public Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, CancellationToken cancellationToken = default) =>
        UpdateHeartbeatAsyncHandler is not null ? UpdateHeartbeatAsyncHandler(screenId, lastSeenUtc, status, cancellationToken) : Task.FromResult(true);

    public Task<int> MarkStaleOnlineScreensOfflineAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default) =>
        MarkStaleOnlineScreensOfflineAsyncHandler is not null
            ? MarkStaleOnlineScreensOfflineAsyncHandler(cutoffUtc, cancellationToken)
            : Task.FromResult(0);
}

internal sealed class FakeScreenPairingCodeRepository : IScreenPairingCodeRepository
{
    public Func<ScreenPairingCode, CancellationToken, Task<string>>? CreateAsyncHandler { get; set; }
    public Func<string, CancellationToken, Task<ScreenPairingCode?>>? GetByCodeAsyncHandler { get; set; }
    public Func<string, Guid, CancellationToken, Task<bool>>? ClaimAsyncHandler { get; set; }
    public ScreenPairingCode? LastCreatedPairingCode { get; private set; }

    public Task<string> CreateAsync(ScreenPairingCode pairingCode, CancellationToken cancellationToken = default)
    {
        LastCreatedPairingCode = pairingCode;
        if (CreateAsyncHandler is not null) return CreateAsyncHandler(pairingCode, cancellationToken);
        return Task.FromResult(pairingCode.Code);
    }

    public Task<ScreenPairingCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        GetByCodeAsyncHandler is not null ? GetByCodeAsyncHandler(code, cancellationToken) : Task.FromResult<ScreenPairingCode?>(null);

    public Task<bool> ClaimAsync(string code, Guid venueId, CancellationToken cancellationToken = default) =>
        ClaimAsyncHandler is not null ? ClaimAsyncHandler(code, venueId, cancellationToken) : Task.FromResult(true);
}

internal sealed class FakeVenueSubscriptionRepository : IVenueSubscriptionRepository
{
    public IReadOnlyCollection<VenueSubscription> Items { get; init; } = [];

    public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items);
    public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(item => item.VenueId == venueId));
    public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(item => item.StripeSubscriptionId == stripeSubscriptionId));
    public Task<bool> SaveAsync(VenueSubscription subscription, CancellationToken cancellationToken = default) => Task.FromResult(true);
}

internal sealed class FakeSubscriptionTierRepository : ISubscriptionTierRepository
{
    public IReadOnlyCollection<SubscriptionTier> Items { get; set; } = [];

    public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items);
    public Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(item => item.Id == tierId));
    public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(item => item.Slug == slug));
    public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
    public Task<bool> CreateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default)
    {
        Items = [.. Items, tier];
        return Task.FromResult(true);
    }
    public Task<bool> UpdateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default)
    {
        Items = [.. Items.Where(item => item.Id != tier.Id), tier];
        return Task.FromResult(true);
    }
}

internal sealed class FakeVenueFeatureOverrideRepository : IVenueFeatureOverrideRepository
{
    public IReadOnlyCollection<VenueFeatureOverride> Items { get; set; } = [];

    public Task<IReadOnlyCollection<VenueFeatureOverride>> GetActiveByVenueAsync(Guid venueId, DateTime utcNow, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<VenueFeatureOverride>>(Items.Where(item =>
            item.VenueId == venueId && (!item.ExpiresAt.HasValue || item.ExpiresAt > utcNow)).ToArray());
    public Task UpsertAsync(VenueFeatureOverride featureOverride, CancellationToken cancellationToken = default)
    {
        Items = [.. Items.Where(item => item.VenueId != featureOverride.VenueId || item.FeatureId != featureOverride.FeatureId), featureOverride];
        return Task.CompletedTask;
    }
    public Task<bool> RemoveAsync(Guid venueId, Guid featureId, CancellationToken cancellationToken = default)
    {
        var updated = Items.Where(item => item.VenueId != venueId || item.FeatureId != featureId).ToArray();
        var removed = updated.Length != Items.Count;
        Items = updated;
        return Task.FromResult(removed);
    }
}

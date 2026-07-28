using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class FeatureMatrixServiceTests
{
    private readonly Guid tierId = Guid.NewGuid();
    private readonly Guid featureId = Guid.NewGuid();

    [Fact]
    public async Task GetAsync_ReturnsOnlyActiveFeaturesInCategoryOrder()
    {
        var active = new Feature { Id = featureId, Key = "z", Label = "Zeta", Category = "display", IsActive = true };
        var first = new Feature { Id = Guid.NewGuid(), Key = "a", Label = "Alpha", Category = "ai", IsActive = true };
        var inactive = new Feature { Id = Guid.NewGuid(), Key = "old", Label = "Old", Category = "ai", IsActive = false };
        var service = CreateService(
            [active, inactive, first],
            [new SubscriptionTier { Id = tierId, Name = "Pro", Price = 89, IsActive = true }]);

        var snapshot = await service.GetAsync();

        Assert.Equal(new[] { first.Id, active.Id }, snapshot.Features.Select(feature => feature.Id));
        Assert.DoesNotContain(snapshot.Features, feature => feature.Id == inactive.Id);
    }

    [Fact]
    public async Task ApplyAsync_PersistsEffectiveChangesAndInvalidatesAffectedVenues()
    {
        var venueId = Guid.NewGuid();
        var matrix = new MatrixRepositoryFake { ChangedCount = 1 };
        var resolver = new FeatureResolutionFake();
        var service = CreateService(
            [new Feature { Id = featureId, Key = "analytics", Label = "Analytics", Category = "analytics", IsActive = true }],
            [new SubscriptionTier { Id = tierId, Name = "Pro", IsActive = true }],
            matrix,
            new SubscriptionRepositoryFake([new VenueSubscription { VenueId = venueId, TierId = tierId }]),
            resolver);

        var changed = await service.ApplyAsync(
            [new FeatureMatrixChange(tierId, featureId, true)],
            " super-admin ");

        Assert.Equal(1, changed);
        Assert.Equal("super-admin", matrix.AdminId);
        Assert.Equal(new DateTime(2026, 7, 28, 22, 0, 0, DateTimeKind.Utc), matrix.ChangedUtc);
        Assert.Equal(venueId, Assert.Single(resolver.InvalidatedVenueIds));
    }

    [Fact]
    public async Task ApplyAsync_RejectsDuplicateCellsBeforePersistence()
    {
        var change = new FeatureMatrixChange(tierId, featureId, true);
        var matrix = new MatrixRepositoryFake();
        var service = CreateService(
            [new Feature { Id = featureId, IsActive = true }],
            [new SubscriptionTier { Id = tierId }],
            matrix);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ApplyAsync([change, change], "admin"));
        Assert.Empty(matrix.AppliedChanges);
    }

    [Fact]
    public async Task ApplyAsync_RejectsInactiveFeatureBeforePersistence()
    {
        var matrix = new MatrixRepositoryFake();
        var service = CreateService(
            [new Feature { Id = featureId, IsActive = false }],
            [new SubscriptionTier { Id = tierId }],
            matrix);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ApplyAsync([new FeatureMatrixChange(tierId, featureId, true)], "admin"));
        Assert.Empty(matrix.AppliedChanges);
    }

    private FeatureMatrixService CreateService(
        IReadOnlyCollection<Feature> features,
        IReadOnlyCollection<SubscriptionTier> tiers,
        MatrixRepositoryFake? matrix = null,
        IVenueSubscriptionRepository? subscriptions = null,
        IFeatureResolutionService? resolver = null) =>
        new(
            new FeatureRepositoryFake(features),
            new TierRepositoryFake(tiers),
            matrix ?? new MatrixRepositoryFake(),
            subscriptions ?? new SubscriptionRepositoryFake([]),
            resolver ?? new FeatureResolutionFake(),
            new FixedTimeProvider());

    private sealed class FeatureRepositoryFake(IReadOnlyCollection<Feature> items) : IFeatureRepository
    {
        public Task<IReadOnlyCollection<Feature>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(items);
        public Task<Feature?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(items.FirstOrDefault(item => item.Key == key));
    }

    private sealed class TierRepositoryFake(IReadOnlyCollection<SubscriptionTier> items) : ISubscriptionTierRepository
    {
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(items);
        public Task<SubscriptionTier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(items.FirstOrDefault(item => item.Id == id));
        public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.FromResult(items.FirstOrDefault(item => item.Slug == slug));
        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
        public Task<bool> CreateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class MatrixRepositoryFake : IFeatureMatrixRepository
    {
        public int ChangedCount { get; init; }
        public IReadOnlyCollection<FeatureMatrixChange> AppliedChanges { get; private set; } = [];
        public string? AdminId { get; private set; }
        public DateTime ChangedUtc { get; private set; }
        public Task<IReadOnlyCollection<TierFeature>> GetAllTierFeaturesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
        public Task<IReadOnlyCollection<FeatureMatrixAuditEntry>> GetRecentAuditAsync(int count, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<FeatureMatrixAuditEntry>>([]);
        public Task<int> ApplyAsync(IReadOnlyCollection<FeatureMatrixChange> changes, string adminId, DateTime changedUtc, CancellationToken cancellationToken = default)
        {
            AppliedChanges = changes; AdminId = adminId; ChangedUtc = changedUtc;
            return Task.FromResult(ChangedCount);
        }
    }

    private sealed class SubscriptionRepositoryFake(IReadOnlyCollection<VenueSubscription> items) : IVenueSubscriptionRepository
    {
        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(items);
        public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult(items.FirstOrDefault(item => item.VenueId == venueId));
        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) => Task.FromResult<VenueSubscription?>(null);
        public Task<bool> SaveAsync(VenueSubscription subscription, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class FeatureResolutionFake : IFeatureResolutionService
    {
        public List<Guid> InvalidatedVenueIds { get; } = [];
        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult<FeatureEntitlement?>(null);
        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(new Dictionary<string, FeatureEntitlement>());
        public void Invalidate(Guid venueId) => InvalidatedVenueIds.Add(venueId);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 28, 22, 0, 0, TimeSpan.Zero);
    }
}

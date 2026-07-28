using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class VenueFeatureOverrideManagementServiceTests
{
    private readonly Guid venueId = Guid.NewGuid();
    private readonly Guid featureId = Guid.NewGuid();

    [Fact]
    public async Task SetAsync_TrimsPersistsAndInvalidates()
    {
        var repository = new FakeVenueFeatureOverrideRepository();
        var resolver = new FeatureResolutionFake();
        var service = CreateService(repository, resolver);

        var result = await service.SetAsync(
            venueId,
            featureId,
            new VenueFeatureOverrideRequest(true, "  Support unlock  ", new DateTime(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc)));

        Assert.NotNull(result);
        Assert.Equal("Support unlock", result.Reason);
        Assert.Same(result, Assert.Single(repository.Items));
        Assert.Equal(venueId, Assert.Single(resolver.InvalidatedVenueIds));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SetAsync_RejectsBlankReason(string reason)
    {
        var service = CreateService(new FakeVenueFeatureOverrideRepository(), new FeatureResolutionFake());
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SetAsync(venueId, featureId, new VenueFeatureOverrideRequest(true, reason, null)));
    }

    [Fact]
    public async Task SetAsync_RejectsPastExpiry()
    {
        var service = CreateService(new FakeVenueFeatureOverrideRepository(), new FeatureResolutionFake());
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SetAsync(
                venueId,
                featureId,
                new VenueFeatureOverrideRequest(true, "Support", new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc))));
    }

    [Fact]
    public async Task SetAsync_ReturnsNullWithoutMutation_WhenFeatureIsInactive()
    {
        var repository = new FakeVenueFeatureOverrideRepository();
        var resolver = new FeatureResolutionFake();
        var service = CreateService(repository, resolver, featureActive: false);

        var result = await service.SetAsync(
            venueId,
            featureId,
            new VenueFeatureOverrideRequest(true, "Support", null));

        Assert.Null(result);
        Assert.Empty(repository.Items);
        Assert.Empty(resolver.InvalidatedVenueIds);
    }

    [Fact]
    public async Task RemoveAsync_RemovesAndInvalidatesExistingOverride()
    {
        var repository = new FakeVenueFeatureOverrideRepository
        {
            Items = [new VenueFeatureOverride { VenueId = venueId, FeatureId = featureId, Reason = "Support" }]
        };
        var resolver = new FeatureResolutionFake();
        var service = CreateService(repository, resolver);

        var removed = await service.RemoveAsync(venueId, featureId);

        Assert.True(removed is true);
        Assert.Empty(repository.Items);
        Assert.Equal(venueId, Assert.Single(resolver.InvalidatedVenueIds));
    }

    private VenueFeatureOverrideManagementService CreateService(
        FakeVenueFeatureOverrideRepository repository,
        FeatureResolutionFake resolver,
        bool featureActive = true) =>
        new(
            new FakeVenueRepository
            {
                GetByIdAsyncHandler = (id, _) => Task.FromResult<Venue?>(id == venueId ? new Venue { Id = venueId } : null)
            },
            new FeatureRepositoryFake([new Feature { Id = featureId, IsActive = featureActive }]),
            repository,
            resolver,
            new FixedTimeProvider());

    private sealed class FeatureRepositoryFake(IReadOnlyCollection<Feature> features) : IFeatureRepository
    {
        public Task<IReadOnlyCollection<Feature>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(features);
        public Task<Feature?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<Feature?>(null);
    }

    private sealed class FeatureResolutionFake : IFeatureResolutionService
    {
        public List<Guid> InvalidatedVenueIds { get; } = [];
        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult<FeatureEntitlement?>(null);
        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(new Dictionary<string, FeatureEntitlement>());
        public void Invalidate(Guid id) => InvalidatedVenueIds.Add(id);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 28, 22, 30, 0, TimeSpan.Zero);
    }
}

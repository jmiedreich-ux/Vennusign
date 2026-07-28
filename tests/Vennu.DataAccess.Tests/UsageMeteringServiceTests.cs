using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public class UsageMeteringServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 28, 14, 30, 0, TimeSpan.Zero);
    private static readonly Guid FeatureId = Guid.Parse("20000000-0000-0000-0000-000000000008");

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Get_usage_returns_monthly_limited_snapshot()
    {
        var venueId = Guid.NewGuid();
        var repository = new UsageRepositoryFake
        {
            Current = new FeatureUsage
            {
                VenueId = venueId,
                FeatureId = FeatureId,
                PeriodStartUtc = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                UsageCount = 7
            }
        };
        var service = CreateService(repository, new FeatureEntitlement("ai_translation", true, "20", "tier"));

        var usage = await service.GetUsageAsync(venueId, "AI_TRANSLATION");

        Assert.Equal(7, usage.Used);
        Assert.Equal(20, usage.Limit);
        Assert.Equal(13, usage.Remaining);
        Assert.Equal(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), usage.PeriodStartUtc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Consume_usage_returns_updated_unlimited_snapshot()
    {
        var repository = new UsageRepositoryFake();
        var service = CreateService(repository, new FeatureEntitlement("ai_translation", true, null, "tier"));

        var usage = await service.ConsumeAsync(Guid.NewGuid(), "ai_translation", 3);

        Assert.Equal(3, usage.Used);
        Assert.Null(usage.Limit);
        Assert.Null(usage.Remaining);
        Assert.Null(repository.LastLimit);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Consume_usage_rejects_exhausted_limit()
    {
        var repository = new UsageRepositoryFake { RejectConsumption = true };
        var service = CreateService(repository, new FeatureEntitlement("ai_translation", true, "1", "tier"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsumeAsync(Guid.NewGuid(), "ai_translation"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Disabled_feature_cannot_be_metered()
    {
        var service = CreateService(
            new UsageRepositoryFake(),
            new FeatureEntitlement("ai_translation", false, "20", "tier"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetUsageAsync(Guid.NewGuid(), "ai_translation"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Invalid_numeric_limit_is_rejected()
    {
        var service = CreateService(
            new UsageRepositoryFake(),
            new FeatureEntitlement("ai_translation", true, "many", "tier"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetUsageAsync(Guid.NewGuid(), "ai_translation"));
    }

    private static UsageMeteringService CreateService(
        UsageRepositoryFake usageRepository,
        FeatureEntitlement entitlement) =>
        new(
            new FeatureRepositoryFake(),
            usageRepository,
            new FeatureResolutionFake(entitlement),
            new FixedTimeProvider(UtcNow));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class FeatureRepositoryFake : IFeatureRepository
    {
        public Task<IReadOnlyCollection<Feature>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Feature>>([]);

        public Task<Feature?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult<Feature?>(key == "ai_translation"
                ? new Feature { Id = FeatureId, Key = key, IsActive = true }
                : null);
    }

    private sealed class UsageRepositoryFake : IFeatureUsageRepository
    {
        public FeatureUsage? Current { get; set; }
        public bool RejectConsumption { get; set; }
        public int? LastLimit { get; private set; }

        public Task<FeatureUsage?> GetAsync(
            Guid venueId,
            Guid featureId,
            DateTime periodStartUtc,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Current);

        public Task<FeatureUsage?> TryConsumeAsync(
            Guid venueId,
            Guid featureId,
            DateTime periodStartUtc,
            int amount,
            int? limit,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            LastLimit = limit;
            if (RejectConsumption)
            {
                return Task.FromResult<FeatureUsage?>(null);
            }

            Current ??= new FeatureUsage
            {
                VenueId = venueId,
                FeatureId = featureId,
                PeriodStartUtc = periodStartUtc
            };
            Current.UsageCount += amount;
            Current.UpdatedUtc = utcNow;
            return Task.FromResult<FeatureUsage?>(Current);
        }
    }

    private sealed class FeatureResolutionFake(FeatureEntitlement entitlement) : IFeatureResolutionService
    {
        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(entitlement.Enabled);

        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<FeatureEntitlement?>(entitlement);

        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(
                new Dictionary<string, FeatureEntitlement> { [entitlement.Key] = entitlement });

        public void Invalidate(Guid venueId)
        {
        }
    }
}

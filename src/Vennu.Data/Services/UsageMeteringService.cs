using System.Globalization;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class UsageMeteringService : IUsageMeteringService
{
    private readonly IFeatureRepository featureRepository;
    private readonly IFeatureUsageRepository usageRepository;
    private readonly IFeatureResolutionService featureResolutionService;
    private readonly TimeProvider timeProvider;

    public UsageMeteringService(
        IFeatureRepository featureRepository,
        IFeatureUsageRepository usageRepository,
        IFeatureResolutionService featureResolutionService,
        TimeProvider timeProvider)
    {
        this.featureRepository = featureRepository;
        this.usageRepository = usageRepository;
        this.featureResolutionService = featureResolutionService;
        this.timeProvider = timeProvider;
    }

    public async Task<FeatureUsageSnapshot> GetUsageAsync(
        Guid venueId,
        string featureKey,
        CancellationToken cancellationToken = default)
    {
        var context = await ResolveContextAsync(venueId, featureKey, cancellationToken).ConfigureAwait(false);
        var usage = await usageRepository.GetAsync(
            venueId,
            context.FeatureId,
            context.PeriodStartUtc,
            cancellationToken).ConfigureAwait(false);

        return CreateSnapshot(context.FeatureKey, context.PeriodStartUtc, usage?.UsageCount ?? 0, context.Limit);
    }

    public async Task<FeatureUsageSnapshot> ConsumeAsync(
        Guid venueId,
        string featureKey,
        int amount = 1,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Usage amount must be positive.");
        }

        var context = await ResolveContextAsync(venueId, featureKey, cancellationToken).ConfigureAwait(false);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var usage = await usageRepository.TryConsumeAsync(
            venueId,
            context.FeatureId,
            context.PeriodStartUtc,
            amount,
            context.Limit,
            utcNow,
            cancellationToken).ConfigureAwait(false);

        if (usage is null)
        {
            throw new InvalidOperationException($"The monthly usage limit for feature '{context.FeatureKey}' has been reached.");
        }

        return CreateSnapshot(context.FeatureKey, context.PeriodStartUtc, usage.UsageCount, context.Limit);
    }

    private async Task<UsageContext> ResolveContextAsync(
        Guid venueId,
        string featureKey,
        CancellationToken cancellationToken)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(featureKey);
        var normalizedKey = featureKey.Trim().ToLowerInvariant();
        var feature = await featureRepository.GetByKeyAsync(normalizedKey, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Feature '{normalizedKey}' does not exist.");
        var entitlement = await featureResolutionService.GetFeatureAsync(venueId, normalizedKey, cancellationToken).ConfigureAwait(false);
        if (entitlement?.Enabled != true)
        {
            throw new InvalidOperationException($"Feature '{normalizedKey}' is not enabled for this venue.");
        }

        int? limit = null;
        if (!string.IsNullOrWhiteSpace(entitlement.LimitValue))
        {
            if (!int.TryParse(entitlement.LimitValue, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedLimit) ||
                parsedLimit < 0)
            {
                throw new InvalidOperationException($"Feature '{normalizedKey}' has an invalid usage limit.");
            }

            limit = parsedLimit;
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var periodStartUtc = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return new UsageContext(feature.Id, normalizedKey, periodStartUtc, limit);
    }

    private static FeatureUsageSnapshot CreateSnapshot(
        string featureKey,
        DateTime periodStartUtc,
        int used,
        int? limit) =>
        new(featureKey, periodStartUtc, used, limit, limit is null ? null : Math.Max(0, limit.Value - used));

    private sealed record UsageContext(Guid FeatureId, string FeatureKey, DateTime PeriodStartUtc, int? Limit);
}

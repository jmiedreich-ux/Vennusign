using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class FeatureMatrixService(
    IFeatureRepository featureRepository,
    ISubscriptionTierRepository tierRepository,
    IFeatureMatrixRepository matrixRepository,
    IVenueSubscriptionRepository subscriptionRepository,
    IFeatureResolutionService featureResolutionService,
    TimeProvider timeProvider) : IFeatureMatrixService
{
    private const int RecentAuditCount = 50;

    public async Task<FeatureMatrixSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var tiersTask = tierRepository.GetAllAsync(cancellationToken);
        var featuresTask = featureRepository.GetAllAsync(cancellationToken);
        var enabledTask = matrixRepository.GetAllTierFeaturesAsync(cancellationToken);
        var auditTask = matrixRepository.GetRecentAuditAsync(RecentAuditCount, cancellationToken);
        await Task.WhenAll(tiersTask, featuresTask, enabledTask, auditTask).ConfigureAwait(false);

        return new FeatureMatrixSnapshot(
            tiersTask.Result
                .OrderByDescending(tier => tier.IsActive)
                .ThenBy(tier => tier.Price)
                .ThenBy(tier => tier.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            featuresTask.Result
                .Where(feature => feature.IsActive)
                .OrderBy(feature => feature.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(feature => feature.Label, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            enabledTask.Result,
            auditTask.Result);
    }

    public async Task<int> ApplyAsync(
        IReadOnlyCollection<FeatureMatrixChange> changes,
        string adminId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminId);
        if (changes.Count == 0) return 0;

        var duplicate = changes
            .GroupBy(change => (change.TierId, change.FeatureId))
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException("A feature matrix cell may only be changed once.", nameof(changes));
        }

        var tiersTask = tierRepository.GetAllAsync(cancellationToken);
        var featuresTask = featureRepository.GetAllAsync(cancellationToken);
        await Task.WhenAll(tiersTask, featuresTask).ConfigureAwait(false);
        var tierIds = tiersTask.Result.Select(tier => tier.Id).ToHashSet();
        var featureIds = featuresTask.Result.Where(feature => feature.IsActive).Select(feature => feature.Id).ToHashSet();

        if (changes.Any(change => !tierIds.Contains(change.TierId)))
        {
            throw new ArgumentException("One or more subscription tiers do not exist.", nameof(changes));
        }
        if (changes.Any(change => !featureIds.Contains(change.FeatureId)))
        {
            throw new ArgumentException("One or more features do not exist or are inactive.", nameof(changes));
        }

        var normalizedAdminId = adminId.Trim();
        if (normalizedAdminId.Length > 150)
        {
            throw new ArgumentException("The admin identifier is too long.", nameof(adminId));
        }

        var changed = await matrixRepository.ApplyAsync(
            changes,
            normalizedAdminId,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);

        if (changed > 0)
        {
            var changedTierIds = changes.Select(change => change.TierId).ToHashSet();
            var subscriptions = await subscriptionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            foreach (var subscription in subscriptions.Where(subscription => changedTierIds.Contains(subscription.TierId)))
            {
                featureResolutionService.Invalidate(subscription.VenueId);
            }
        }

        return changed;
    }
}

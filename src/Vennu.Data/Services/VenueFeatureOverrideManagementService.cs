using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class VenueFeatureOverrideManagementService(
    IVenueRepository venueRepository,
    IFeatureRepository featureRepository,
    IVenueFeatureOverrideRepository overrideRepository,
    IOperationalEventRepository operationalEventRepository,
    IFeatureResolutionService featureResolutionService,
    TimeProvider timeProvider) : IVenueFeatureOverrideManagementService
{
    public async Task<VenueFeatureOverride?> SetAsync(
        Guid venueId,
        Guid featureId,
        VenueFeatureOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length == 0) throw new ArgumentException("An override reason is required.", nameof(request));
        if (reason.Length > 500) throw new ArgumentException("Override reason cannot exceed 500 characters.", nameof(request));

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        if (request.ExpiresAt is not null && request.ExpiresAt <= utcNow)
        {
            throw new ArgumentException("Override expiry must be in the future.", nameof(request));
        }

        if (!await TargetsExistAsync(venueId, featureId, cancellationToken).ConfigureAwait(false)) return null;
        var featureOverride = new VenueFeatureOverride
        {
            VenueId = venueId,
            FeatureId = featureId,
            Enabled = request.Enabled,
            Reason = reason,
            ExpiresAt = request.ExpiresAt,
            CreatedUtc = utcNow
        };
        await overrideRepository.UpsertAsync(featureOverride, cancellationToken).ConfigureAwait(false);
        featureResolutionService.Invalidate(venueId);
        await operationalEventRepository.AddAsync(
            new OperationalEvent
            {
                Id = Guid.NewGuid(),
                VenueId = venueId,
                EventType = "override_applied",
                Summary = $"{(request.Enabled ? "Unlocked" : "Blocked")} feature {featureId}: {reason}",
                OccurredUtc = utcNow
            },
            cancellationToken).ConfigureAwait(false);
        return featureOverride;
    }

    public async Task<bool?> RemoveAsync(Guid venueId, Guid featureId, CancellationToken cancellationToken = default)
    {
        if (!await TargetsExistAsync(venueId, featureId, cancellationToken).ConfigureAwait(false)) return null;
        var removed = await overrideRepository.RemoveAsync(venueId, featureId, cancellationToken).ConfigureAwait(false);
        if (removed)
        {
            featureResolutionService.Invalidate(venueId);
            await operationalEventRepository.AddAsync(
                new OperationalEvent
                {
                    Id = Guid.NewGuid(),
                    VenueId = venueId,
                    EventType = "override_removed",
                    Summary = $"Removed feature override {featureId}",
                    OccurredUtc = timeProvider.GetUtcNow().UtcDateTime
                },
                cancellationToken).ConfigureAwait(false);
        }
        return removed;
    }

    private async Task<bool> TargetsExistAsync(Guid venueId, Guid featureId, CancellationToken cancellationToken)
    {
        var venueTask = venueRepository.GetByIdAsync(venueId, cancellationToken);
        var featuresTask = featureRepository.GetAllAsync(cancellationToken);
        await Task.WhenAll(venueTask, featuresTask).ConfigureAwait(false);
        return venueTask.Result is not null && featuresTask.Result.Any(feature => feature.Id == featureId && feature.IsActive);
    }
}

using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IFeatureUsageRepository
{
    Task<FeatureUsage?> GetAsync(
        Guid venueId,
        Guid featureId,
        DateTime periodStartUtc,
        CancellationToken cancellationToken = default);

    Task<FeatureUsage?> TryConsumeAsync(
        Guid venueId,
        Guid featureId,
        DateTime periodStartUtc,
        int amount,
        int? limit,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}

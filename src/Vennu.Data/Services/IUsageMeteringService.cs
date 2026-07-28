namespace Vennu.Data.Services;

public interface IUsageMeteringService
{
    Task<FeatureUsageSnapshot> GetUsageAsync(
        Guid venueId,
        string featureKey,
        CancellationToken cancellationToken = default);

    Task<FeatureUsageSnapshot> ConsumeAsync(
        Guid venueId,
        string featureKey,
        int amount = 1,
        CancellationToken cancellationToken = default);
}

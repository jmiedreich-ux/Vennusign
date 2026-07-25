namespace Vennu.Data.Services;

public interface IFeatureResolutionService
{
    Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default);
    Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default);
    void Invalidate(Guid venueId);
}

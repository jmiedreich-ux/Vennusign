using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public class VenueFeatureOverrideRepository : IVenueFeatureOverrideRepository
{
    private readonly ISqlDataAccess dataAccess;

    public VenueFeatureOverrideRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<IReadOnlyCollection<VenueFeatureOverride>> GetActiveByVenueAsync(Guid venueId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var overrides = await dataAccess.QueryAsync<VenueFeatureOverride, object>(
            "dbo.VenueFeatureOverrides",
            new { VenueId = venueId },
            cancellationToken).ConfigureAwait(false);

        return overrides.Where(item => item.ExpiresAt is null || item.ExpiresAt > utcNow).ToArray();
    }

    public async Task UpsertAsync(VenueFeatureOverride featureOverride, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureOverride);
        if (string.IsNullOrWhiteSpace(featureOverride.Reason))
        {
            throw new ArgumentException("An override reason is required.", nameof(featureOverride));
        }

        featureOverride.CreatedUtc = featureOverride.CreatedUtc == default ? DateTime.UtcNow : featureOverride.CreatedUtc;
        await dataAccess.MergeAllAsync(
            new[] { featureOverride },
            "dbo.VenueFeatureOverrides",
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}

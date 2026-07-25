using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public class VenueFeatureOverrideRepository : IVenueFeatureOverrideRepository
{
    private readonly ISqlDataAccess dataAccess;

    public VenueFeatureOverrideRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<IReadOnlyCollection<VenueFeatureOverride>> GetActiveByVenueAsync(Guid venueId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT VenueId, FeatureId, Enabled, Reason, ExpiresAt, CreatedByAdminId, CreatedUtc
            FROM dbo.VenueFeatureOverrides
            WHERE VenueId = @VenueId AND (ExpiresAt IS NULL OR ExpiresAt > @UtcNow)
            """;

        return (await dataAccess.QueryAsync<VenueFeatureOverride>(sql, new { VenueId = venueId, UtcNow = utcNow }, cancellationToken).ConfigureAwait(false)).ToArray();
    }

    public Task UpsertAsync(VenueFeatureOverride featureOverride, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureOverride);
        if (string.IsNullOrWhiteSpace(featureOverride.Reason))
        {
            throw new ArgumentException("An override reason is required.", nameof(featureOverride));
        }

        const string sql = """
            MERGE dbo.VenueFeatureOverrides AS target
            USING (SELECT @VenueId AS VenueId, @FeatureId AS FeatureId) AS source
               ON target.VenueId = source.VenueId AND target.FeatureId = source.FeatureId
            WHEN MATCHED THEN UPDATE SET Enabled = @Enabled, Reason = @Reason, ExpiresAt = @ExpiresAt, CreatedByAdminId = @CreatedByAdminId, CreatedUtc = @CreatedUtc
            WHEN NOT MATCHED THEN INSERT (VenueId, FeatureId, Enabled, Reason, ExpiresAt, CreatedByAdminId, CreatedUtc)
                VALUES (@VenueId, @FeatureId, @Enabled, @Reason, @ExpiresAt, @CreatedByAdminId, @CreatedUtc);
            """;

        featureOverride.CreatedUtc = featureOverride.CreatedUtc == default ? DateTime.UtcNow : featureOverride.CreatedUtc;
        return dataAccess.ExecuteAsync(sql, featureOverride, cancellationToken);
    }

    public Task DeleteAsync(Guid venueId, Guid featureId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM dbo.VenueFeatureOverrides WHERE VenueId = @VenueId AND FeatureId = @FeatureId";
        return dataAccess.ExecuteAsync(sql, new { VenueId = venueId, FeatureId = featureId }, cancellationToken);
    }
}

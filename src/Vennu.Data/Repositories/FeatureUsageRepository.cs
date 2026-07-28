using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class FeatureUsageRepository : IFeatureUsageRepository
{
    private const string ConsumeSql = """
        MERGE dbo.FeatureUsages WITH (HOLDLOCK) AS target
        USING (SELECT @VenueId AS VenueId, @FeatureId AS FeatureId, @PeriodStartUtc AS PeriodStartUtc) AS source
          ON target.VenueId = source.VenueId
         AND target.FeatureId = source.FeatureId
         AND target.PeriodStartUtc = source.PeriodStartUtc
        WHEN MATCHED AND (@Limit IS NULL OR target.UsageCount + @Amount <= @Limit)
          THEN UPDATE
            SET UsageCount = target.UsageCount + @Amount,
                UpdatedUtc = @UtcNow
        WHEN NOT MATCHED AND (@Limit IS NULL OR @Amount <= @Limit)
          THEN INSERT (VenueId, FeatureId, PeriodStartUtc, UsageCount, CreatedUtc, UpdatedUtc)
               VALUES (@VenueId, @FeatureId, @PeriodStartUtc, @Amount, @UtcNow, @UtcNow)
        OUTPUT inserted.VenueId,
               inserted.FeatureId,
               inserted.PeriodStartUtc,
               inserted.UsageCount,
               inserted.CreatedUtc,
               inserted.UpdatedUtc;
        """;

    private readonly ISqlDataAccess dataAccess;

    public FeatureUsageRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public Task<FeatureUsage?> GetAsync(
        Guid venueId,
        Guid featureId,
        DateTime periodStartUtc,
        CancellationToken cancellationToken = default) =>
        dataAccess.QueryAsync<FeatureUsage>(
            new { VenueId = venueId, FeatureId = featureId, PeriodStartUtc = periodStartUtc },
            cancellationToken);

    public async Task<FeatureUsage?> TryConsumeAsync(
        Guid venueId,
        Guid featureId,
        DateTime periodStartUtc,
        int amount,
        int? limit,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<FeatureUsage, object>(
            ConsumeSql,
            new { VenueId = venueId, FeatureId = featureId, PeriodStartUtc = periodStartUtc, Amount = amount, Limit = limit, UtcNow = utcNow },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
}

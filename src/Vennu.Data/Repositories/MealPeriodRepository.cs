using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class MealPeriodRepository(ISqlDataAccess dataAccess) : IMealPeriodRepository
{
    private const string ByVenueSql = """
        SELECT Id, VenueId, Name, StartLocalTime, EndLocalTime, ActiveDaysMask,
               IsEnabled, TargetLayout, MenuFilter, ThemePresetKey,
               SortOrder, CreatedUtc, UpdatedUtc
        FROM dbo.MealPeriods
        WHERE VenueId = @VenueId
        ORDER BY SortOrder, Id;
        """;

    private const string DeleteSql = """
        DELETE FROM dbo.MealPeriods
        WHERE VenueId = @VenueId AND Id = @MealPeriodId;
        SELECT CONVERT(BIT, CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END) AS Removed;
        """;

    public async Task<Guid> CreateAsync(
        MealPeriod mealPeriod,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mealPeriod);
        RequireVenueId(mealPeriod.VenueId);

        if (mealPeriod.Id == Guid.Empty)
        {
            mealPeriod.Id = Guid.NewGuid();
        }

        var now = DateTime.UtcNow;
        if (mealPeriod.CreatedUtc == default)
        {
            mealPeriod.CreatedUtc = now;
        }

        if (mealPeriod.UpdatedUtc == default)
        {
            mealPeriod.UpdatedUtc = now;
        }

        await dataAccess.InsertAsync(mealPeriod, cancellationToken).ConfigureAwait(false);
        return mealPeriod.Id;
    }

    public async Task<bool> UpdateAsync(
        MealPeriod mealPeriod,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mealPeriod);
        RequireVenueId(mealPeriod.VenueId);
        return await dataAccess.UpdateAsync(mealPeriod, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<IReadOnlyCollection<MealPeriod>> GetByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MealPeriod, object>(
            ByVenueSql,
            new { VenueId = RequireVenueId(venueId) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<bool> DeleteAsync(
        Guid venueId,
        Guid mealPeriodId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<RemovalResult, object>(
            DeleteSql,
            new
            {
                VenueId = RequireVenueId(venueId),
                MealPeriodId = mealPeriodId == Guid.Empty
                    ? throw new ArgumentException("Identifier cannot be empty.", nameof(mealPeriodId))
                    : mealPeriodId
            },
            cancellationToken).ConfigureAwait(false)).Single().Removed;

    private static Guid RequireVenueId(Guid venueId) =>
        venueId == Guid.Empty
            ? throw new ArgumentException("Identifier cannot be empty.", nameof(venueId))
            : venueId;

    public sealed class RemovalResult
    {
        public bool Removed { get; set; }
    }
}

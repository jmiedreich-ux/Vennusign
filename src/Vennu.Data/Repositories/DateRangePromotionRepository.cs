using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class DateRangePromotionRepository(ISqlDataAccess dataAccess) : IDateRangePromotionRepository
{
    private const string ByVenueSql = """
        SELECT Id, VenueId, Name, StartLocalDate, EndLocalDate, TargetLayout, Title, Body,
               Priority, IsEnabled, CreatedUtc, UpdatedUtc
        FROM dbo.DateRangePromotions
        WHERE VenueId = @VenueId
        ORDER BY StartLocalDate DESC, Priority DESC, CreatedUtc DESC, Id;
        """;

    public async Task<IReadOnlyCollection<DateRangePromotion>> GetByVenueAsync(
        Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<DateRangePromotion, object>(
            ByVenueSql, new { VenueId = Require(venueId) }, cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<Guid> CreateAsync(DateRangePromotion promotion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);
        if (promotion.Id == Guid.Empty) promotion.Id = Guid.NewGuid();
        await dataAccess.InsertAsync(promotion, cancellationToken).ConfigureAwait(false);
        return promotion.Id;
    }

    public async Task<bool> UpdateAsync(DateRangePromotion promotion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);
        return await dataAccess.UpdateAsync(promotion, cancellationToken).ConfigureAwait(false) > 0;
    }

    private static Guid Require(Guid id) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.") : id;
}

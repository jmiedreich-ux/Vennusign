using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IDateRangePromotionRepository
{
    Task<IReadOnlyCollection<DateRangePromotion>> GetByVenueAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(DateRangePromotion promotion, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(DateRangePromotion promotion, CancellationToken cancellationToken = default);
}

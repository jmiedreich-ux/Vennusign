using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IDateRangePromotionService
{
    Task<IReadOnlyCollection<DateRangePromotion>> GetAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<DateRangePromotion?> GetActiveAsync(Guid venueId, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
    Task<DateRangePromotion> CreateAsync(Guid venueId, DateRangePromotion promotion, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
    Task<DateRangePromotion?> UpdateAsync(Guid venueId, Guid promotionId, DateRangePromotion promotion, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
    Task<DateRangePromotion?> ArchiveAsync(Guid venueId, Guid promotionId, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
}

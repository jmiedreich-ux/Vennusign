using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IDateRangePromotionResolver
{
    DateRangePromotionResolution Resolve(
        string timezoneId,
        DateTimeOffset utcNow,
        IReadOnlyCollection<DateRangePromotion> promotions);
}

public sealed record DateRangePromotionResolution(
    DateTimeOffset LocalNow,
    DateRangePromotion? ActivePromotion);

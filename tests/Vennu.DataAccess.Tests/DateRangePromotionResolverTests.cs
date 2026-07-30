using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class DateRangePromotionResolverTests
{
    private readonly DateRangePromotionResolver resolver = new();

    [Fact]
    public void Resolve_UsesVenueLocalDateAndInclusiveBoundaries()
    {
        var promotion = Promotion(
            "Late-night launch",
            new DateTime(2026, 7, 29),
            new DateTime(2026, 7, 29));

        var result = resolver.Resolve(
            "America/Los_Angeles",
            new DateTimeOffset(2026, 7, 30, 6, 30, 0, TimeSpan.Zero),
            [promotion]);

        Assert.Equal(new DateTime(2026, 7, 29), result.LocalNow.Date);
        Assert.Same(promotion, result.ActivePromotion);
    }

    [Fact]
    public void Resolve_UsesPriorityThenMostRecentStartForOverlaps()
    {
        var older = Promotion("Older", new DateTime(2026, 7, 1), new DateTime(2026, 7, 31), priority: 5);
        var newer = Promotion("Newer", new DateTime(2026, 7, 20), new DateTime(2026, 7, 31), priority: 5);
        var lower = Promotion("Lower", new DateTime(2026, 7, 29), new DateTime(2026, 7, 31), priority: 4);

        var result = resolver.Resolve(
            "UTC",
            new DateTimeOffset(2026, 7, 30, 12, 0, 0, TimeSpan.Zero),
            [lower, older, newer]);

        Assert.Same(newer, result.ActivePromotion);
    }

    [Fact]
    public void Resolve_ExcludesDisabledAndExpiredPromotions()
    {
        var disabled = Promotion("Disabled", new DateTime(2026, 7, 1), new DateTime(2026, 7, 31));
        disabled.IsEnabled = false;
        var expired = Promotion("Expired", new DateTime(2026, 7, 1), new DateTime(2026, 7, 29));

        var result = resolver.Resolve(
            "UTC",
            new DateTimeOffset(2026, 7, 30, 12, 0, 0, TimeSpan.Zero),
            [disabled, expired]);

        Assert.Null(result.ActivePromotion);
    }

    [Fact]
    public void Resolve_RejectsUnknownTimezone()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => resolver.Resolve("Not/A_Timezone", DateTimeOffset.UtcNow, []));

        Assert.Equal("timezoneId", exception.ParamName);
    }

    private static DateRangePromotion Promotion(
        string name,
        DateTime start,
        DateTime end,
        int priority = 0) =>
        new()
        {
            Id = Guid.NewGuid(),
            VenueId = Guid.NewGuid(),
            Name = name,
            StartLocalDate = start,
            EndLocalDate = end,
            Priority = priority,
            IsEnabled = true,
            CreatedUtc = new DateTime(2026, 7, 1)
        };
}

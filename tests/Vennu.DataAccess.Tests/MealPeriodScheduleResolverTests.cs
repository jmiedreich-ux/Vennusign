using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public sealed class MealPeriodScheduleResolverTests
{
    private readonly MealPeriodScheduleResolver resolver = new();

    [Fact]
    [Trait("Category", "Unit")]
    public void Resolve_UsesVenueTimezoneAcrossDstBoundary()
    {
        var period = Period("After jump", 3, 4, DayOfWeek.Sunday);

        var result = resolver.Resolve(
            "America/New_York",
            new DateTimeOffset(2026, 3, 8, 7, 30, 0, TimeSpan.Zero),
            [period]);

        Assert.Equal(new TimeSpan(-4, 0, 0), result.LocalNow.Offset);
        Assert.Equal(new TimeSpan(3, 30, 0), result.LocalNow.TimeOfDay);
        Assert.Same(period, result.ActiveMealPeriod);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Resolve_AssignsAfterMidnightPartOfOvernightWindowToPreviousDay()
    {
        var overnight = Period("Late menu", 22, 2, DayOfWeek.Friday);

        var result = resolver.Resolve(
            "UTC",
            new DateTimeOffset(2026, 8, 1, 1, 0, 0, TimeSpan.Zero),
            [overnight]);

        Assert.Same(overnight, result.ActiveMealPeriod);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Resolve_UsesSortOrderThenIdentifierForOverlaps()
    {
        var laterId = Period("Lunch", 11, 14, DayOfWeek.Thursday, sortOrder: 2, id: Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"));
        var earlierId = Period("Special", 11, 14, DayOfWeek.Thursday, sortOrder: 2, id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var lowerPriority = Period("Backup", 11, 14, DayOfWeek.Thursday, sortOrder: 3);

        var result = resolver.Resolve(
            "UTC",
            new DateTimeOffset(2026, 7, 30, 12, 0, 0, TimeSpan.Zero),
            [laterId, lowerPriority, earlierId]);

        Assert.Same(earlierId, result.ActiveMealPeriod);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Resolve_ExcludesDisabledAndInactiveDays()
    {
        var disabled = Period("Disabled", 8, 12, DayOfWeek.Thursday);
        disabled.IsEnabled = false;
        var fridayOnly = Period("Friday", 8, 12, DayOfWeek.Friday);

        var result = resolver.Resolve(
            "UTC",
            new DateTimeOffset(2026, 7, 30, 9, 0, 0, TimeSpan.Zero),
            [disabled, fridayOnly]);

        Assert.Null(result.ActiveMealPeriod);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Resolve_RejectsUnknownTimezone()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => resolver.Resolve("Not/A_Timezone", DateTimeOffset.UtcNow, []));

        Assert.Equal("timezoneId", exception.ParamName);
    }

    private static MealPeriod Period(
        string name,
        int startHour,
        int endHour,
        DayOfWeek activeDay,
        int sortOrder = 0,
        Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            VenueId = Guid.NewGuid(),
            Name = name,
            StartLocalTime = TimeSpan.FromHours(startHour),
            EndLocalTime = TimeSpan.FromHours(endHour),
            ActiveDaysMask = 1 << (int)activeDay,
            IsEnabled = true,
            SortOrder = sortOrder
        };
}

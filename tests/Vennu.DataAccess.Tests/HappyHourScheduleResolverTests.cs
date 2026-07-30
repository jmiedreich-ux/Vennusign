using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class HappyHourScheduleResolverTests
{
    private readonly HappyHourScheduleResolver resolver = new();

    [Fact]
    public void Resolve_AutomaticSupportsOvernightWindows()
    {
        var schedule = Schedule(22, 2, DayOfWeek.Wednesday);
        var result = resolver.Resolve("UTC", new DateTimeOffset(2026, 7, 30, 1, 0, 0, TimeSpan.Zero), schedule);

        Assert.True(result.IsActive);
        Assert.Equal(HappyHourOverrideMode.Automatic, result.Mode);
        Assert.Equal(new DateTimeOffset(2026, 7, 30, 2, 0, 0, TimeSpan.Zero), result.EndsAtUtc);
    }

    [Theory]
    [InlineData(HappyHourOverrideMode.ForceOn, true)]
    [InlineData(HappyHourOverrideMode.ForceOff, false)]
    public void Resolve_ManualOverrideWins(string mode, bool expected)
    {
        var schedule = Schedule(16, 19, DayOfWeek.Monday);
        schedule.OverrideMode = mode;

        Assert.Equal(expected, resolver.Resolve("UTC", new DateTimeOffset(2026, 7, 30, 8, 0, 0, TimeSpan.Zero), schedule).IsActive);
    }

    private static HappyHourSchedule Schedule(int start, int end, DayOfWeek day) => new()
    {
        VenueId = Guid.NewGuid(),
        StartLocalTime = TimeSpan.FromHours(start),
        EndLocalTime = TimeSpan.FromHours(end),
        ActiveDaysMask = 1 << (int)day,
        IsEnabled = true
    };
}

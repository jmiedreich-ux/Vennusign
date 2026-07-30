using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class MealPeriodAdministrationService(
    IMealPeriodRepository repository,
    TimeProvider timeProvider) : IMealPeriodAdministrationService
{
    private const int MinutesPerDay = 1440;
    private const int MinutesPerWeek = MinutesPerDay * 7;

    public async Task<MealPeriodAdministrationSnapshot> GetAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        var periods = await repository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var conflicts = periods
            .SelectMany((first, index) => periods.Skip(index + 1)
                .Where(second => first.IsEnabled && second.IsEnabled && Overlaps(first, second))
                .Select(second => new MealPeriodConflict(first.Id, first.Name, second.Id, second.Name)))
            .ToArray();
        return new MealPeriodAdministrationSnapshot(periods, conflicts);
    }

    public async Task<MealPeriod> CreateAsync(
        Guid venueId,
        string name,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        Validate(venueId, name, startLocalTime, endLocalTime, activeDaysMask);
        var existing = await repository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var period = new MealPeriod
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Name = name.Trim(),
            StartLocalTime = startLocalTime,
            EndLocalTime = endLocalTime,
            ActiveDaysMask = activeDaysMask,
            IsEnabled = isEnabled,
            SortOrder = existing.Count == 0 ? 0 : existing.Max(item => item.SortOrder) + 1,
            CreatedUtc = now,
            UpdatedUtc = now
        };
        await repository.CreateAsync(period, cancellationToken).ConfigureAwait(false);
        return period;
    }

    public async Task<MealPeriod?> UpdateAsync(
        Guid venueId,
        Guid mealPeriodId,
        string name,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        Validate(venueId, name, startLocalTime, endLocalTime, activeDaysMask);
        RequireId(mealPeriodId, nameof(mealPeriodId));
        var periods = await repository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var period = periods.SingleOrDefault(item => item.Id == mealPeriodId);
        if (period is null)
        {
            return null;
        }

        period.Name = name.Trim();
        period.StartLocalTime = startLocalTime;
        period.EndLocalTime = endLocalTime;
        period.ActiveDaysMask = activeDaysMask;
        period.IsEnabled = isEnabled;
        period.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await repository.UpdateAsync(period, cancellationToken).ConfigureAwait(false);
        return period;
    }

    public Task<bool> DeleteAsync(
        Guid venueId,
        Guid mealPeriodId,
        CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        RequireId(mealPeriodId, nameof(mealPeriodId));
        return repository.DeleteAsync(venueId, mealPeriodId, cancellationToken);
    }

    private static void Validate(
        Guid venueId,
        string name,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask)
    {
        RequireId(venueId, nameof(venueId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Trim().Length > 100) throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        if (startLocalTime < TimeSpan.Zero || startLocalTime >= TimeSpan.FromDays(1)) throw new ArgumentOutOfRangeException(nameof(startLocalTime));
        if (endLocalTime < TimeSpan.Zero || endLocalTime >= TimeSpan.FromDays(1)) throw new ArgumentOutOfRangeException(nameof(endLocalTime));
        if (startLocalTime == endLocalTime) throw new ArgumentException("Start and end times must differ.");
        if (activeDaysMask is < 1 or > 127) throw new ArgumentOutOfRangeException(nameof(activeDaysMask));
    }

    private static bool Overlaps(MealPeriod first, MealPeriod second)
    {
        var firstWindows = Windows(first);
        var secondWindows = Windows(second);
        return firstWindows.Any(a => secondWindows.Any(b =>
            Intersects(a, b)
            || Intersects(a, (b.Start - MinutesPerWeek, b.End - MinutesPerWeek))
            || Intersects(a, (b.Start + MinutesPerWeek, b.End + MinutesPerWeek))));
    }

    private static IReadOnlyCollection<(int Start, int End)> Windows(MealPeriod period) =>
        Enumerable.Range(0, 7)
            .Where(day => (period.ActiveDaysMask & (1 << day)) != 0)
            .Select(day =>
            {
                var start = day * MinutesPerDay + (int)period.StartLocalTime.TotalMinutes;
                var end = day * MinutesPerDay + (int)period.EndLocalTime.TotalMinutes;
                return (start, end <= start ? end + MinutesPerDay : end);
            })
            .ToArray();

    private static bool Intersects((int Start, int End) first, (int Start, int End) second) =>
        first.Start < second.End && second.Start < first.End;

    private static void RequireId(Guid id, string name)
    {
        if (id == Guid.Empty) throw new ArgumentException("Identifier cannot be empty.", name);
    }
}

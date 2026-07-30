using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class PlaylistAdministrationService(
    IPlaylistSlideRepository repository,
    IScreenRepository screens,
    IVenueRepository venues,
    TimeProvider timeProvider) : IPlaylistAdministrationService
{
    public async Task<IReadOnlyCollection<PlaylistSlide>> GetAsync(
        Guid venueId, Guid screenId, CancellationToken cancellationToken = default)
    {
        await RequireScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        return await repository.GetByScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<PlaylistSlide>> GetActiveAsync(
        Guid venueId, Guid screenId, DateTimeOffset utcNow, CancellationToken cancellationToken = default)
    {
        var venue = await venues.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Venue does not exist.");
        var slides = await GetAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        var localNow = TimeZoneInfo.ConvertTime(utcNow.ToUniversalTime(), ResolveTimezone(venue.Timezone));
        return slides.Where(slide => IsActive(slide, localNow)).ToArray();
    }

    public async Task<PlaylistSlide> CreateAsync(
        Guid venueId, Guid screenId, PlaylistSlideWrite write, CancellationToken cancellationToken = default)
    {
        await RequireScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        var existing = await repository.GetByScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var slide = Build(write, new PlaylistSlide
        {
            Id = Guid.NewGuid(), VenueId = venueId, ScreenId = screenId,
            SortOrder = existing.Count == 0 ? 0 : existing.Max(item => item.SortOrder) + 1,
            CreatedUtc = now
        }, now);
        await repository.CreateAsync(slide, cancellationToken).ConfigureAwait(false);
        return slide;
    }

    public async Task<PlaylistSlide?> UpdateAsync(
        Guid venueId, Guid screenId, Guid slideId, PlaylistSlideWrite write,
        CancellationToken cancellationToken = default)
    {
        var slides = await GetAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        var slide = slides.SingleOrDefault(item => item.Id == Require(slideId));
        if (slide is null) return null;
        Build(write, slide, timeProvider.GetUtcNow().UtcDateTime);
        await repository.UpdateAsync(slide, cancellationToken).ConfigureAwait(false);
        return slide;
    }

    public async Task<bool> DeleteAsync(
        Guid venueId, Guid screenId, Guid slideId, CancellationToken cancellationToken = default)
    {
        await RequireScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        return await repository.DeleteAsync(venueId, screenId, Require(slideId), cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<PlaylistSlide>> ReorderAsync(
        Guid venueId, Guid screenId, IReadOnlyCollection<Guid> orderedIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderedIds);
        var slides = (await GetAsync(venueId, screenId, cancellationToken).ConfigureAwait(false)).ToArray();
        if (orderedIds.Count != slides.Length || orderedIds.Distinct().Count() != slides.Length
            || orderedIds.Any(id => slides.All(slide => slide.Id != id)))
            throw new ArgumentException("Reorder must contain every slide exactly once.", nameof(orderedIds));
        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var pair in orderedIds.Select((id, index) => (id, index)))
        {
            var slide = slides.Single(item => item.Id == pair.id);
            slide.SortOrder = pair.index;
            slide.UpdatedUtc = now;
            await repository.UpdateAsync(slide, cancellationToken).ConfigureAwait(false);
        }
        return slides.OrderBy(item => item.SortOrder).ToArray();
    }

    private async Task RequireScreenAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken)
    {
        Require(venueId); Require(screenId);
        var screen = await screens.GetByIdAsync(screenId, cancellationToken).ConfigureAwait(false);
        if (screen?.VenueId != venueId) throw new KeyNotFoundException("Screen does not exist for this venue.");
    }

    private static PlaylistSlide Build(PlaylistSlideWrite write, PlaylistSlide slide, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(write);
        var type = PlaylistSlideType.Normalize(write.SlideType);
        if (write.DwellSeconds is < 5 or > 120) throw new ArgumentOutOfRangeException(nameof(write.DwellSeconds));
        var hasStart = write.StartLocalTime.HasValue;
        if (hasStart != write.EndLocalTime.HasValue || hasStart != write.ActiveDaysMask.HasValue)
            throw new ArgumentException("Optional window requires start, end, and active days together.");
        if (hasStart)
        {
            if (write.StartLocalTime == write.EndLocalTime) throw new ArgumentException("Window times must differ.");
            if (write.ActiveDaysMask is < 1 or > 127) throw new ArgumentOutOfRangeException(nameof(write.ActiveDaysMask));
        }
        var title = Normalize(write.Title, 200);
        var body = Normalize(write.Body, 1000);
        var media = Normalize(write.MediaUrl, 1000);
        if (type == PlaylistSlideType.Image && media is null) throw new ArgumentException("Image slides require a media URL.");
        if (type == PlaylistSlideType.Message && title is null && body is null) throw new ArgumentException("Message slides require title or body.");
        slide.SlideType = type; slide.Title = title; slide.Body = body; slide.MediaUrl = media;
        slide.DwellSeconds = write.DwellSeconds; slide.StartLocalTime = write.StartLocalTime;
        slide.EndLocalTime = write.EndLocalTime; slide.ActiveDaysMask = write.ActiveDaysMask;
        slide.IsEnabled = write.IsEnabled; slide.UpdatedUtc = now;
        return slide;
    }

    private static bool IsActive(PlaylistSlide slide, DateTimeOffset localNow)
    {
        if (!slide.IsEnabled) return false;
        if (!slide.StartLocalTime.HasValue) return true;
        var start = slide.StartLocalTime.Value; var end = slide.EndLocalTime!.Value;
        return start < end
            ? IsDay(slide.ActiveDaysMask!.Value, localNow.DayOfWeek) && localNow.TimeOfDay >= start && localNow.TimeOfDay < end
            : localNow.TimeOfDay >= start
                ? IsDay(slide.ActiveDaysMask!.Value, localNow.DayOfWeek)
                : localNow.TimeOfDay < end && IsDay(slide.ActiveDaysMask!.Value, localNow.AddDays(-1).DayOfWeek);
    }

    private static bool IsDay(int mask, DayOfWeek day) => (mask & (1 << (int)day)) != 0;
    private static string? Normalize(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= max ? value.Trim() : throw new ArgumentException($"Value cannot exceed {max} characters.");
    private static Guid Require(Guid id) => id == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.") : id;
    private static TimeZoneInfo ResolveTimezone(string timezoneId)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(timezoneId); }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        { throw new ArgumentException($"Timezone '{timezoneId}' is invalid.", nameof(timezoneId), exception); }
    }
}

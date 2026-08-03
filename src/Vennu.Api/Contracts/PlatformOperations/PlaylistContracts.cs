using Vennu.Data.Services;

namespace Vennu.Api.Contracts.PlatformOperations;

public sealed record PlaylistSlideWriteRequest(
    string SlideType, string? Title, string? Body, string? MediaUrl, int DwellSeconds,
    TimeSpan? StartLocalTime, TimeSpan? EndLocalTime, int? ActiveDaysMask, bool IsEnabled)
{
    public PlaylistSlideWrite ToWrite() =>
        new(SlideType, Title, Body, MediaUrl, DwellSeconds, StartLocalTime, EndLocalTime, ActiveDaysMask, IsEnabled);
}

public sealed record PlaylistReorderRequest(IReadOnlyCollection<Guid> OrderedIds);

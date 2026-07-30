namespace Vennu.Core.Models;

public sealed class PlaylistSlide
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public Guid ScreenId { get; set; }
    public string SlideType { get; set; } = PlaylistSlideType.Menu;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? MediaUrl { get; set; }
    public int DwellSeconds { get; set; } = 10;
    public TimeSpan? StartLocalTime { get; set; }
    public TimeSpan? EndLocalTime { get; set; }
    public int? ActiveDaysMask { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

public static class PlaylistSlideType
{
    public const string Menu = "menu";
    public const string Image = "image";
    public const string Message = "message";

    public static string Normalize(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            Menu => Menu,
            Image => Image,
            Message => Message,
            _ => throw new ArgumentException("Slide type must be menu, image, or message.", nameof(value))
        };
}

namespace Vennu.Api.Contracts.Display;

public class DisplayContentResponse
{
    public Guid ScreenId { get; set; }

    public Guid? VenueId { get; set; }

    public string ScreenKey { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public string Status { get; set; } = "Offline";

    public DateTime? LastSeenUtc { get; set; }

    public long? ContentRevision { get; set; }

    public string Layout { get; set; } = "default";

    public string? VenueName { get; set; }

    public string? MenuName { get; set; }

    public string? DailySpecial { get; set; }

    public bool IsHappyHour { get; set; }

    public DateTimeOffset? HappyHourEndsAtUtc { get; set; }

    public string HappyHourMode { get; set; } = "automatic";

    public string PhotoGridDensity { get; set; } = "3x2";

    public int PhotoGridOverflowItems { get; set; }

    public string SplitRatio { get; set; } = "40_60";

    public int HeroDwellSeconds { get; set; } = 8;

    public IReadOnlyCollection<DisplayPlaylistSlideResponse> Playlist { get; set; } = [];

    public DisplayEmergencyBroadcastResponse? EmergencyBroadcast { get; set; }

    public DisplayPromotionResponse? Promotion { get; set; }

    public IReadOnlyCollection<Vennu.Core.Models.TapCategory> TapCategories { get; set; } = [];

    public IReadOnlyCollection<Vennu.Core.Models.TapItem> TapItems { get; set; } = [];

    public DisplayThemeResponse Theme { get; set; } = new();

    public IReadOnlyCollection<DisplayMenuSectionResponse> Sections { get; set; } = [];

    /*
     * Every page this screen is assigned, in order, so a screen holding more than one can rotate
     * between them - which the back office has promised on the assignment page all along.
     *
     * `Sections` above stays what it was: the first assigned page. A player that knows nothing of
     * pages keeps working, and one that does uses this instead. That is why this is added beside
     * the old field rather than replacing it.
     */
    public IReadOnlyCollection<DisplayMenuPageResponse> Pages { get; set; } = [];

    /// How long each page holds the screen before the next one, from the menu (Menus.DwellSeconds).
    public int PageDwellSeconds { get; set; } = 12;
}

/// <summary>One assigned page and what it draws.</summary>
public sealed class DisplayMenuPageResponse
{
    public Guid PageId { get; set; }

    public string? Name { get; set; }

    public IReadOnlyCollection<DisplayMenuSectionResponse> Sections { get; set; } = [];
}

public sealed class DisplayPromotionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TargetLayout { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public DateTime EndLocalDate { get; set; }

    public static DisplayPromotionResponse From(Vennu.Core.Models.DateRangePromotion value) =>
        new()
        {
            Id = value.Id, Name = value.Name, TargetLayout = value.TargetLayout,
            Title = value.Title, Body = value.Body, EndLocalDate = value.EndLocalDate
        };
}

public sealed class DisplayEmergencyBroadcastResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime ExpiresUtc { get; set; }

    public static DisplayEmergencyBroadcastResponse From(Vennu.Core.Models.EmergencyBroadcast value) =>
        new() { Id = value.Id, Title = value.Title, Message = value.Message, MediaUrl = value.MediaUrl, ExpiresUtc = value.ExpiresUtc };
}

public sealed class DisplayPlaylistSlideResponse
{
    public Guid Id { get; set; }
    public string SlideType { get; set; } = "menu";
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? MediaUrl { get; set; }
    public int DwellSeconds { get; set; }
}

public sealed class DisplayThemeResponse
{
    public string BackgroundColor { get; set; } = "#111315";

    public string AccentColor { get; set; } = "#FFB74D";

    public string FontFamily { get; set; } = "Inter";

    public string PresetKey { get; set; } = "bar_classic";

    public string TitleColor { get; set; } = "#F8F5E9";

    public string GlowColor { get; set; } = "#00E5FF";

    public string BoardBackgroundColor { get; set; } = "#071013";

    public IReadOnlyCollection<string> SectionColors { get; set; } = ["#00E5FF", "#FF2BD6", "#FFE66D", "#7CFF6B"];

    public decimal GlowIntensity { get; set; } = 1.00m;

    public string TitleFont { get; set; } = "Righteous";

    public string ItemFont { get; set; } = "Caveat";
}

public sealed class DisplayMenuSectionResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<DisplayMenuItemResponse> Items { get; set; } = [];
}

public sealed class DisplayMenuItemResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? HappyHourPrice { get; set; }

    public bool IsAvailable { get; set; }

    public int? QuantityAvailable { get; set; }

    public bool IsPopular { get; set; }

    public IReadOnlyCollection<string> Tags { get; set; } = [];

    public string? ImageUrl { get; set; }
}

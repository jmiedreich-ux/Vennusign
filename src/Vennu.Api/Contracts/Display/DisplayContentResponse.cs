namespace Vennu.Api.Contracts.Display;

public class DisplayContentResponse
{
    public Guid ScreenId { get; set; }

    public Guid? VenueId { get; set; }

    public string ScreenKey { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public string Status { get; set; } = "Offline";

    public DateTime? LastSeenUtc { get; set; }

    public string Layout { get; set; } = "default";

    public string? VenueName { get; set; }

    public string? MenuName { get; set; }

    public bool IsHappyHour { get; set; }

    public string PhotoGridDensity { get; set; } = "3x2";

    public int PhotoGridOverflowItems { get; set; }

    public IReadOnlyCollection<DisplayMenuSectionResponse> Sections { get; set; } = [];
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

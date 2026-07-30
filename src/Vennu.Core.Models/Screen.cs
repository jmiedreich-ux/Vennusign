namespace Vennu.Core.Models;

public class Screen
{
    public Guid Id { get; set; }

    public Guid? VenueId { get; set; }

    public string ScreenKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? WallGroup { get; set; }

    public int? WallPosition { get; set; }

    public string PhotoGridDensity { get; set; } = "3x2";

    public string DisplayLayout { get; set; } = "photo_grid";

    public string SplitRatio { get; set; } = "40_60";

    public DateTime? LastSeen { get; set; }

    public string Status { get; set; } = "Offline";

    public string? Platform { get; set; }

    public string? AppVersion { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

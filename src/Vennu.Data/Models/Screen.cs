namespace Vennu.Data.Models;

public class Screen
{
    public Guid Id { get; set; }

    public Guid? VenueId { get; set; }

    public string ScreenKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? WallGroup { get; set; }

    public int? WallPosition { get; set; }

    public DateTime? LastSeen { get; set; }

    public string Status { get; set; } = "Offline";

    public string? Platform { get; set; }

    public string? AppVersion { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

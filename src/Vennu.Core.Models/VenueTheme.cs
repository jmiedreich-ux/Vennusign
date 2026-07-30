namespace Vennu.Core.Models;

public sealed class VenueTheme
{
    public Guid VenueId { get; set; }

    public string BackgroundColor { get; set; } = "#111315";

    public string AccentColor { get; set; } = "#FFB74D";

    public string FontFamily { get; set; } = "Inter";

    public DateTime UpdatedUtc { get; set; }
}

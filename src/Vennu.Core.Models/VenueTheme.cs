namespace Vennu.Core.Models;

public sealed class VenueTheme
{
    public Guid VenueId { get; set; }

    public string BackgroundColor { get; set; } = "#111315";

    public string AccentColor { get; set; } = "#FFB74D";

    public string FontFamily { get; set; } = "Inter";

    public string PresetKey { get; set; } = "bar_classic";

    public string TitleColor { get; set; } = "#F8F5E9";

    public string GlowColor { get; set; } = "#00E5FF";

    public string BoardBackgroundColor { get; set; } = "#071013";

    public string SectionColors { get; set; } = "#00E5FF,#FF2BD6,#FFE66D,#7CFF6B";

    public decimal GlowIntensity { get; set; } = 1.00m;

    public string TitleFont { get; set; } = "Righteous";

    public string ItemFont { get; set; } = "Caveat";

    public DateTime UpdatedUtc { get; set; }
}

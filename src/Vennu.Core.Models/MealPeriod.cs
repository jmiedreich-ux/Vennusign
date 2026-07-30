namespace Vennu.Core.Models;

public sealed class MealPeriod
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public string Name { get; set; } = string.Empty;

    public TimeSpan StartLocalTime { get; set; }

    public TimeSpan EndLocalTime { get; set; }

    public int ActiveDaysMask { get; set; } = 127;

    public bool IsEnabled { get; set; } = true;

    public string? TargetLayout { get; set; }

    public string? MenuFilter { get; set; }

    public string? ThemePresetKey { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

namespace Vennu.Core.Models;

public sealed class Menu
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string? DailySpecial { get; set; }

    /// <summary>How long each page of the board holds before the next one. Configurable, never a constant.</summary>
    public int DwellSeconds { get; set; } = MenuSettingDefaults.DwellSeconds;

    /// <summary>Full-cycle length past which the board is called too long to read. Configurable.</summary>
    public int LoopWarningSeconds { get; set; } = MenuSettingDefaults.LoopWarningSeconds;

    public string Theme { get; set; } = MenuSettingDefaults.Theme;

    /// <summary>Moved to the "Not in use" strip by a person; it keeps its history and comes back when placed on a screen.</summary>
    public bool IsPutAway { get; set; }

    /// <summary>The version currently on the screens; null until the menu has ever been published.</summary>
    public long? PublishedVersion { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

public static class MenuSettingDefaults
{
    public const int DwellSeconds = 8;

    public const int LoopWarningSeconds = 60;

    public const string Theme = "coastal";

    public const int MinDwellSeconds = 2;

    public const int MaxDwellSeconds = 120;

    public const int MinLoopWarningSeconds = 10;

    public const int MaxLoopWarningSeconds = 900;
}

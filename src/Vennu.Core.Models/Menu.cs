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

    /// <summary>
    /// The menu theme attached to this menu, or null when none is. No theme is a
    /// valid state (Q86): the board renders plainly rather than blank, and never
    /// falls back to something invented. Themes are created in the theme editor
    /// and attached here; the table behind this lands with the milestone that
    /// first reads one.
    /// </summary>
    public string? Theme { get; set; }

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

    // There is deliberately no default theme. A menu with none attached is a
    // valid state, and no named look exists to default to (Q86).

    public const int MinDwellSeconds = 2;

    public const int MaxDwellSeconds = 120;

    public const int MinLoopWarningSeconds = 10;

    public const int MaxLoopWarningSeconds = 900;
}

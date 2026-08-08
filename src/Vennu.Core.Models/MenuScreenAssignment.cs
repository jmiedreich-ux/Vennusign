namespace Vennu.Core.Models;

/// <summary>
/// Which menu a screen is showing. Exactly one menu per screen this milestone,
/// held as its own record so Schedules can point several menus at a screen later
/// without a migration.
/// </summary>
public sealed class MenuScreenAssignment
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid ScreenId { get; set; }

    public Guid MenuId { get; set; }

    public DateTime AssignedUtc { get; set; }

    public string? AssignedBy { get; set; }
}

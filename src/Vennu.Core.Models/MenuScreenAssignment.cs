namespace Vennu.Core.Models;

/// <summary>
/// One page in a screen's ordered rotation. A screen may carry several page
/// assignments; the theme owns the interval while Menus owns page order.
/// </summary>
public sealed class MenuScreenAssignment
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid ScreenId { get; set; }

    public Guid MenuId { get; set; }

    public Guid PageId { get; set; }

    public string? MenuName { get; set; }

    public string? PageName { get; set; }

    public bool Rotate { get; set; }

    public DateTime AssignedUtc { get; set; }

    public string? AssignedBy { get; set; }
}

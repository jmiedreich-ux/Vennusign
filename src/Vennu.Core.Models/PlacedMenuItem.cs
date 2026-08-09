namespace Vennu.Core.Models;

/// <summary>
/// A read model for editing surfaces: an item as it sits on a board, joined with
/// its live availability. Prices stay exactly as typed (Q115/Q190).
/// </summary>
public sealed class PlacedMenuItem
{
    public Guid MenuId { get; set; }

    public Guid MenuSectionId { get; set; }

    public Guid ItemId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Price { get; set; }

    public int SortOrder { get; set; }

    public bool IsAvailable { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

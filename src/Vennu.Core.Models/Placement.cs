namespace Vennu.Core.Models;

/// <summary>
/// An item placed on a section of a menu, in order. Placements are what a board
/// renders; removing one leaves the item in the library.
/// </summary>
public sealed class Placement
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid MenuId { get; set; }

    public Guid MenuSectionId { get; set; }

    public Guid ItemId { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

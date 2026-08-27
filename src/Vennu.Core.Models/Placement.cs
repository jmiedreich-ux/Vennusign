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

    public Guid PageId { get; set; }

    public Guid ItemId { get; set; }

    public int SortOrder { get; set; }

    /// <summary>
    /// What this dish costs on this menu (A19). Named for the import that first
    /// needed it; it is now the price of the placement, and <c>Items.Price</c> is
    /// the default a dish carries when it is placed somewhere new. Null means the
    /// placement predates A19 and still reads through to that default.
    ///
    /// Stored exactly as it was typed (Q115/Q190) - no parsing, no currency.
    /// </summary>
    public string? ImportedPriceOverride { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

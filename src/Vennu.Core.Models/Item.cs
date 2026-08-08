namespace Vennu.Core.Models;

/// <summary>
/// A venue-scoped library item. Items are placed onto boards through
/// <see cref="Placement"/>; the same item can appear on several menus.
/// </summary>
public sealed class Item
{
    public const int NameMaxLength = 200;

    public const int DescriptionMaxLength = 1000;

    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Null when the item has no price yet (an import the parser was unsure about,
    /// or a market-price item). A missing price never blocks a publish.
    /// </summary>
    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public string Source { get; set; } = ItemSources.Manual;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

public static class ItemSources
{
    public const string Manual = "manual";

    public const string Pos = "pos";

    public const string Import = "import";

    public static bool IsSupported(string? value) =>
        value is Manual or Pos or Import;
}

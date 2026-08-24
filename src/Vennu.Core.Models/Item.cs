namespace Vennu.Core.Models;

/// <summary>
/// A venue-scoped library item. Items are placed onto boards through
/// <see cref="Placement"/>; the same item can appear on several menus.
/// </summary>
public sealed class Item
{
    public const int NameMaxLength = 200;

    public const int DescriptionMaxLength = 1000;

    public const int PriceMaxLength = 12;

    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// The price exactly as the operator typed it, and exactly as a board renders
    /// it: "12", "9.5" and "MP" all round-trip unchanged. It is deliberately not a
    /// number - a numeric type would normalise "9.5" and could not hold "MP" at
    /// all. Null means no price yet, which never blocks a publish.
    /// </summary>
    public string? Price { get; set; }

    public string? ImageUrl { get; set; }

    public string Source { get; set; } = ItemSources.Manual;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// A drafted, publish-gated flag: turned off, this item stops shipping to guest
    /// screens on the next publish. Distinct from <see cref="ItemAvailability"/>
    /// ("86"), which commits instantly and never waits for a publish.
    /// </summary>
    public bool IsListed { get; set; } = true;

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

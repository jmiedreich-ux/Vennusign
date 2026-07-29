namespace Vennu.Core.Models;

public sealed class MenuItem
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid MenuSectionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? HappyHourPrice { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int? QuantityAvailable { get; set; }

    public string? Tags { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPopular { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

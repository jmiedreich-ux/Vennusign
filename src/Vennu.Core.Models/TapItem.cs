namespace Vennu.Core.Models;

public sealed class TapItem
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public Guid? TapCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Style { get; set; }
    public decimal? Abv { get; set; }
    public int? Ibu { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? GlassColor { get; set; }
    public string? NameColor { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsComingSoon { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

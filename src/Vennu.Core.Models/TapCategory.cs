namespace Vennu.Core.Models;

public sealed class TapCategory
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? CategoryPrice { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

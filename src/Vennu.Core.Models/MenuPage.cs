namespace Vennu.Core.Models;

public sealed class MenuPage
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public Guid MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

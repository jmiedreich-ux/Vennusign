namespace Vennu.Core.Models;

public sealed class DateRangePromotion
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartLocalDate { get; set; }
    public DateTime EndLocalDate { get; set; }
    public string? TargetLayout { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

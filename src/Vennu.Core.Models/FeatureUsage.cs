namespace Vennu.Core.Models;

public class FeatureUsage
{
    public Guid VenueId { get; set; }
    public Guid FeatureId { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public int UsageCount { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

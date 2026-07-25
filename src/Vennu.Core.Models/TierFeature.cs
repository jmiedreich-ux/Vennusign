namespace Vennu.Core.Models;

public class TierFeature
{
    public Guid TierId { get; set; }
    public Guid FeatureId { get; set; }
    public string? LimitValue { get; set; }
    public DateTime CreatedUtc { get; set; }
}

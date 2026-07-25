namespace Vennu.Core.Models;

public class VenueFeatureOverride
{
    public Guid VenueId { get; set; }
    public Guid FeatureId { get; set; }
    public bool Enabled { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public Guid? CreatedByAdminId { get; set; }
    public DateTime CreatedUtc { get; set; }
}

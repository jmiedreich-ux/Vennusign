namespace Vennu.Core.Models;

public class FeatureMatrixAuditEntry
{
    public Guid Id { get; set; }
    public Guid TierId { get; set; }
    public Guid FeatureId { get; set; }
    public string AdminId { get; set; } = string.Empty;
    public bool PreviousEnabled { get; set; }
    public bool NewEnabled { get; set; }
    public DateTime ChangedUtc { get; set; }
}

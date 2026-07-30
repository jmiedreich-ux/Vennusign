namespace Vennu.Core.Models;

public sealed class EmergencyBroadcast
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public Guid? ScreenId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime StartsUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

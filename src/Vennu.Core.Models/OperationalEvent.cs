namespace Vennu.Core.Models;

public class OperationalEvent
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime OccurredUtc { get; set; }
}

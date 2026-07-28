namespace Vennu.Core.Models;

public class ProcessedStripeEvent
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = "processing";
    public DateTime StartedUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }
    public string? FailureReason { get; set; }
}

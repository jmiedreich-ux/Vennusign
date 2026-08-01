namespace Vennu.Core.Models;

public enum PosWebhookEventStatus
{
    Queued = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}

public sealed class PosWebhookEvent
{
    public Guid Id { get; set; }
    public PosProvider Provider { get; set; }
    public string ProviderEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ExternalMerchantId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public PosWebhookEventStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime ReceivedUtc { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }
    public DateTime? NextAttemptUtc { get; set; }
    public string? FailureReason { get; set; }
}

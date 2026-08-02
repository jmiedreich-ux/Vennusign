namespace Vennu.Core.Models;

public sealed class OrganizationSubscription
{
    public Guid OrganizationId { get; set; }
    public Guid TierId { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string Status { get; set; } = "trialing";
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

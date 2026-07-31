namespace Vennu.Core.Models;

public class HaasContract
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public string BundleKey { get; set; } = string.Empty;
    public int TermMonths { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime StartedUtc { get; set; }
    public DateTime ContractEndsUtc { get; set; }
    public DateTime? EndedUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

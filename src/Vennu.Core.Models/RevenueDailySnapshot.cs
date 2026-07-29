namespace Vennu.Core.Models;

public sealed class RevenueDailySnapshot
{
    public DateTime SnapshotDateUtc { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Mrr { get; set; }
    public decimal Arr { get; set; }
    public decimal AverageRevenuePerActiveSubscription { get; set; }
    public int ActiveSubscriptions { get; set; }
    public DateTime CapturedUtc { get; set; }
}

namespace Vennu.Core.Models;

public class SubscriptionTier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxScreens { get; set; }
    public int MaxVenues { get; set; } = 1;
    public int TrialDays { get; set; } = 14;
    public string TrialExpiryBehavior { get; set; } = "disable";
    public bool IsPublic { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? StripeProductId { get; set; }
    public string? StripeMonthlyPriceId { get; set; }
    public string? StripeAnnualPriceId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

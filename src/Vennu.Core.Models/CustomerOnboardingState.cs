namespace Vennu.Core.Models;

public sealed class CustomerOnboardingState
{
    public Guid UserId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? SelectedTierId { get; set; }
    public Guid? VenueId { get; set; }
    public Guid? FirstScreenId { get; set; }

    /// <summary>
    /// When this account's first screen was first seen Online. Latched once and never
    /// cleared: a display that later goes offline does not un-complete onboarding.
    /// </summary>
    public DateTime? GoLiveAchievedUtc { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

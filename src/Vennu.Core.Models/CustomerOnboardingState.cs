namespace Vennu.Core.Models;

public sealed class CustomerOnboardingState
{
    public Guid UserId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? SelectedTierId { get; set; }
    public Guid? VenueId { get; set; }
    public Guid? FirstScreenId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

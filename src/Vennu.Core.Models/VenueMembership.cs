namespace Vennu.Core.Models;

public enum VenueMembershipRole
{
    Manager = 1,
    Editor = 2,
    Viewer = 3
}

public sealed class VenueMembership
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid VenueId { get; set; }
    public Guid UserId { get; set; }
    public VenueMembershipRole Role { get; set; }
    public DateTime GrantedUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

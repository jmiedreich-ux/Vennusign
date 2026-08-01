namespace Vennu.Core.Models;

public enum MembershipCapability
{
    ReadOrganization = 1,
    ManageOrganizationMembers = 2,
    TransferOrganizationOwnership = 3,
    ReadVenue = 4,
    ManageVenueMembers = 5,
    ManageVenueContent = 6
}

public enum MembershipAuditScope
{
    Organization = 1,
    Venue = 2
}

public enum MembershipAuditAction
{
    OrganizationCreated = 1,
    OrganizationMemberAdded = 2,
    OrganizationMemberRoleChanged = 3,
    OrganizationMemberRevoked = 4,
    OrganizationOwnershipTransferred = 5,
    VenueAttached = 6,
    VenueMemberAdded = 7,
    VenueMemberRoleChanged = 8,
    VenueMemberRevoked = 9
}

public sealed class MembershipAuditEntry
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? VenueId { get; set; }
    public Guid ActorUserId { get; set; }
    public Guid SubjectUserId { get; set; }
    public MembershipAuditScope Scope { get; set; }
    public MembershipAuditAction Action { get; set; }
    public string? PreviousRole { get; set; }
    public string? NewRole { get; set; }
    public DateTime OccurredUtc { get; set; }
}

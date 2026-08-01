namespace Vennu.Core.Models;

public enum OrganizationMembershipRole
{
    Owner = 1,
    Admin = 2,
    Member = 3
}

public sealed class OrganizationMembership
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public OrganizationMembershipRole Role { get; set; }
    public DateTime JoinedUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

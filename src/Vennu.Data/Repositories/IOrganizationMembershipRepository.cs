using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IOrganizationMembershipRepository
{
    Task<OrganizationMembership?> GetOrganizationMembershipAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<VenueMembership?> GetVenueMembershipAsync(
        Guid organizationId,
        Guid venueId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Organization> CreateOrganizationAsync(
        Organization organization,
        OrganizationMembership ownerMembership,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default);

    Task<OrganizationMembership> SaveOrganizationMembershipAsync(
        OrganizationMembership membership,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default);

    Task TransferOwnershipAsync(
        Guid organizationId,
        Guid currentOwnerUserId,
        Guid newOwnerUserId,
        DateTime occurredUtc,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default);

    Task AttachVenueAsync(
        Guid organizationId,
        Guid venueId,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default);

    Task<VenueMembership> SaveVenueMembershipAsync(
        VenueMembership membership,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default);
}

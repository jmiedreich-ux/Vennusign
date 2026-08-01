using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IIdentityMembershipService
{
    Task<Organization> CreateOrganizationAsync(
        string name,
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    Task<OrganizationMembership> AddOrChangeOrganizationMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid subjectUserId,
        OrganizationMembershipRole role,
        CancellationToken cancellationToken = default);

    Task RevokeOrganizationMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid subjectUserId,
        CancellationToken cancellationToken = default);

    Task TransferOwnershipAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid newOwnerUserId,
        CancellationToken cancellationToken = default);

    Task AttachVenueAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<VenueMembership> AddOrChangeVenueMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        Guid subjectUserId,
        VenueMembershipRole role,
        CancellationToken cancellationToken = default);

    Task RevokeVenueMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        Guid subjectUserId,
        CancellationToken cancellationToken = default);
}

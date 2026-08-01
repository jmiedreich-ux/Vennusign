using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IMembershipCapabilityResolver
{
    IReadOnlySet<MembershipCapability> Resolve(
        OrganizationMembershipRole? organizationRole,
        VenueMembershipRole? venueRole);

    bool HasCapability(
        OrganizationMembershipRole? organizationRole,
        VenueMembershipRole? venueRole,
        MembershipCapability capability);
}

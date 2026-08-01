using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed class MembershipCapabilityResolver : IMembershipCapabilityResolver
{
    public IReadOnlySet<MembershipCapability> Resolve(
        OrganizationMembershipRole? organizationRole,
        VenueMembershipRole? venueRole)
    {
        var capabilities = new HashSet<MembershipCapability>();

        if (organizationRole is not null)
        {
            if (!Enum.IsDefined(organizationRole.Value))
                throw new ArgumentOutOfRangeException(nameof(organizationRole));

            capabilities.Add(MembershipCapability.ReadOrganization);
            switch (organizationRole)
            {
                case OrganizationMembershipRole.Owner:
                    capabilities.Add(MembershipCapability.TransferOrganizationOwnership);
                    goto case OrganizationMembershipRole.Admin;
                case OrganizationMembershipRole.Admin:
                    capabilities.Add(MembershipCapability.ManageOrganizationMembers);
                    capabilities.Add(MembershipCapability.ReadVenue);
                    capabilities.Add(MembershipCapability.ManageVenueMembers);
                    capabilities.Add(MembershipCapability.ManageVenueContent);
                    break;
                case OrganizationMembershipRole.Member:
                    break;
            }
        }

        if (venueRole is not null)
        {
            if (!Enum.IsDefined(venueRole.Value))
                throw new ArgumentOutOfRangeException(nameof(venueRole));

            capabilities.Add(MembershipCapability.ReadVenue);
            switch (venueRole)
            {
                case VenueMembershipRole.Manager:
                    capabilities.Add(MembershipCapability.ManageVenueMembers);
                    capabilities.Add(MembershipCapability.ManageVenueContent);
                    break;
                case VenueMembershipRole.Editor:
                    capabilities.Add(MembershipCapability.ManageVenueContent);
                    break;
                case VenueMembershipRole.Viewer:
                    break;
            }
        }

        return capabilities;
    }

    public bool HasCapability(
        OrganizationMembershipRole? organizationRole,
        VenueMembershipRole? venueRole,
        MembershipCapability capability)
    {
        if (!Enum.IsDefined(capability))
            throw new ArgumentOutOfRangeException(nameof(capability));
        return Resolve(organizationRole, venueRole).Contains(capability);
    }
}

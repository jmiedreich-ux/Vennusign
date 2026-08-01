using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class MembershipCapabilityResolverTests
{
    private readonly MembershipCapabilityResolver resolver = new();

    [Fact]
    public void Owner_ReceivesEveryOrganizationAndVenueCapability()
    {
        var capabilities = resolver.Resolve(OrganizationMembershipRole.Owner, null);

        Assert.Equal(Enum.GetValues<MembershipCapability>().Order(), capabilities.Order());
    }

    [Fact]
    public void OrganizationMember_ReceivesOnlyOrganizationRead()
    {
        var capabilities = resolver.Resolve(OrganizationMembershipRole.Member, null);

        Assert.Equal([MembershipCapability.ReadOrganization], capabilities);
    }

    [Theory]
    [InlineData(VenueMembershipRole.Manager, true, true)]
    [InlineData(VenueMembershipRole.Editor, false, true)]
    [InlineData(VenueMembershipRole.Viewer, false, false)]
    public void VenueRoles_MapToDeterministicCapabilities(
        VenueMembershipRole role,
        bool managesMembers,
        bool managesContent)
    {
        var capabilities = resolver.Resolve(OrganizationMembershipRole.Member, role);

        Assert.Contains(MembershipCapability.ReadOrganization, capabilities);
        Assert.Contains(MembershipCapability.ReadVenue, capabilities);
        Assert.Equal(managesMembers, capabilities.Contains(MembershipCapability.ManageVenueMembers));
        Assert.Equal(managesContent, capabilities.Contains(MembershipCapability.ManageVenueContent));
        Assert.DoesNotContain(MembershipCapability.TransferOrganizationOwnership, capabilities);
    }

    [Fact]
    public void UndefinedRole_IsRejectedInsteadOfGrantingCapabilities()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            resolver.Resolve((OrganizationMembershipRole)999, null));
    }
}

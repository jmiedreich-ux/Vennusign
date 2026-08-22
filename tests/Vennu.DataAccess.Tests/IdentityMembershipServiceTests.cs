using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class IdentityMembershipServiceTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SubjectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 22, 45, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateOrganizationAsync_CreatesOwnerMembershipAndAudit()
    {
        var identities = new FakeIdentityRepository { User = new CustomerUser { Id = OwnerId, Status = CustomerUserStatus.Active } };
        var memberships = new FakeMembershipRepository();
        var service = CreateService(identities, memberships);

        var organization = await service.CreateOrganizationAsync(" Customer Organization ", OwnerId);

        Assert.Equal("Customer Organization", organization.Name);
        Assert.Equal(OwnerId, organization.OwnerUserId);
        Assert.Equal(OrganizationMembershipRole.Owner, memberships.SavedOrganizationMembership!.Role);
        Assert.Equal(MembershipAuditAction.OrganizationCreated, memberships.LastAudit!.Action);
        Assert.Equal(UtcNow.UtcDateTime, memberships.LastAudit.OccurredUtc);
    }

    [Fact]
    public async Task CreateOrganizationAsync_NormalizesRequiredBusinessProfile()
    {
        var identities = new FakeIdentityRepository { User = new CustomerUser { Id = OwnerId, Status = CustomerUserStatus.Active } };
        var memberships = new FakeMembershipRepository();
        var service = CreateService(identities, memberships);

        var organization = await service.CreateOrganizationAsync(
            new OrganizationProfile(" Vennusign Cafe ", " Vennusign Cafe LLC ", " Alex Owner ", " OWNER@EXAMPLE.COM ", " 555-0100 ", " 1 Main St, New York, NY 10001 "), OwnerId);

        Assert.Equal("Vennusign Cafe", organization.Name);
        Assert.Equal("Vennusign Cafe LLC", organization.LegalName);
        Assert.Equal("Alex Owner", organization.PrimaryContactName);
        Assert.Equal("owner@example.com", organization.ContactEmail);
        Assert.Equal("1 Main St, New York, NY 10001", organization.MailingAddress);
    }

    [Fact]
    public async Task Admin_AddsOrganizationMember_WithImmutableAuditEvidence()
    {
        var identities = new FakeIdentityRepository { User = new CustomerUser { Id = SubjectId, Status = CustomerUserStatus.Active } };
        var memberships = new FakeMembershipRepository
        {
            OrganizationMembershipHandler = userId => userId == OwnerId
                ? ActiveOrganizationMember(OwnerId, OrganizationMembershipRole.Admin)
                : null
        };
        var service = CreateService(identities, memberships);

        var result = await service.AddOrChangeOrganizationMemberAsync(
            OwnerId, OrganizationId, SubjectId, OrganizationMembershipRole.Member);

        Assert.Equal(OrganizationMembershipRole.Member, result.Role);
        Assert.Equal(MembershipAuditAction.OrganizationMemberAdded, memberships.LastAudit!.Action);
        Assert.Equal(OwnerId, memberships.LastAudit.ActorUserId);
        Assert.Equal(SubjectId, memberships.LastAudit.SubjectUserId);
    }

    [Fact]
    public async Task AddOrganizationMember_RejectsDirectOwnerAssignment()
    {
        var service = CreateService(new FakeIdentityRepository(), new FakeMembershipRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddOrChangeOrganizationMemberAsync(
                OwnerId, OrganizationId, SubjectId, OrganizationMembershipRole.Owner));
    }

    [Fact]
    public async Task RevokeOrganizationMember_RejectsActiveOwner()
    {
        var memberships = new FakeMembershipRepository
        {
            OrganizationMembershipHandler = userId => ActiveOrganizationMember(userId, OrganizationMembershipRole.Owner)
        };
        var service = CreateService(new FakeIdentityRepository(), memberships);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RevokeOrganizationMemberAsync(OwnerId, OrganizationId, OwnerId));
    }

    [Fact]
    public async Task VenueManager_CanGrantVenueEditor_WhenSubjectBelongsToOrganization()
    {
        var venueId = Guid.NewGuid();
        var memberships = new FakeMembershipRepository
        {
            OrganizationMembershipHandler = userId => ActiveOrganizationMember(userId, OrganizationMembershipRole.Member),
            VenueMembershipHandler = userId => userId == OwnerId
                ? ActiveVenueMember(venueId, OwnerId, VenueMembershipRole.Manager)
                : null
        };
        var service = CreateService(new FakeIdentityRepository(), memberships);

        var result = await service.AddOrChangeVenueMemberAsync(
            OwnerId, OrganizationId, venueId, SubjectId, VenueMembershipRole.Editor);

        Assert.Equal(VenueMembershipRole.Editor, result.Role);
        Assert.Equal(MembershipAuditAction.VenueMemberAdded, memberships.LastAudit!.Action);
        Assert.Equal(MembershipAuditScope.Venue, memberships.LastAudit.Scope);
    }

    [Fact]
    public async Task OrganizationMember_CannotManageVenueMembersWithoutVenueManagerRole()
    {
        var memberships = new FakeMembershipRepository
        {
            OrganizationMembershipHandler = userId => ActiveOrganizationMember(userId, OrganizationMembershipRole.Member)
        };
        var service = CreateService(new FakeIdentityRepository(), memberships);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AddOrChangeVenueMemberAsync(
                OwnerId, OrganizationId, Guid.NewGuid(), SubjectId, VenueMembershipRole.Viewer));
    }

    private static IdentityMembershipService CreateService(
        FakeIdentityRepository identities,
        FakeMembershipRepository memberships) =>
        new(identities, memberships, new MembershipCapabilityResolver(), new FixedTimeProvider(UtcNow));

    private static OrganizationMembership ActiveOrganizationMember(Guid userId, OrganizationMembershipRole role) => new()
    {
        Id = Guid.NewGuid(), OrganizationId = OrganizationId, UserId = userId, Role = role, JoinedUtc = UtcNow.UtcDateTime
    };

    private static VenueMembership ActiveVenueMember(Guid venueId, Guid userId, VenueMembershipRole role) => new()
    {
        Id = Guid.NewGuid(), OrganizationId = OrganizationId, VenueId = venueId, UserId = userId,
        Role = role, GrantedUtc = UtcNow.UtcDateTime
    };

    private sealed class FakeIdentityRepository : ICustomerIdentityRepository
    {
        public CustomerUser? User { get; set; }
        public Task<CustomerUser> CreateUserAsync(CustomerUser user, CancellationToken cancellationToken = default) => Task.FromResult(user);
        public Task<CustomerUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(User);
        public Task<CustomerUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(User);
        public Task<ExternalIdentityLinkResult> UpsertExternalIdentityAsync(ExternalIdentity identity, bool allowSubjectChange, CancellationToken cancellationToken = default) => Task.FromResult(new ExternalIdentityLinkResult(identity, false));
        public Task<ExternalIdentity?> GetExternalIdentityAsync(ExternalIdentityProvider provider, string providerSubject, CancellationToken cancellationToken = default) => Task.FromResult<ExternalIdentity?>(null);
    }

    private sealed class FakeMembershipRepository : IOrganizationMembershipRepository
    {
        public Task<Organization?> GetOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default) => Task.FromResult<Organization?>(null);
        public Func<Guid, OrganizationMembership?> OrganizationMembershipHandler { get; set; } = _ => null;
        public Func<Guid, VenueMembership?> VenueMembershipHandler { get; set; } = _ => null;
        public OrganizationMembership? SavedOrganizationMembership { get; private set; }
        public MembershipAuditEntry? LastAudit { get; private set; }

        public Task<OrganizationMembership?> GetOrganizationMembershipAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(OrganizationMembershipHandler(userId));

        public Task<VenueMembership?> GetVenueMembershipAsync(Guid organizationId, Guid venueId, Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(VenueMembershipHandler(userId));

        public Task<Organization> CreateOrganizationAsync(Organization organization, OrganizationMembership ownerMembership, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default)
        {
            SavedOrganizationMembership = ownerMembership;
            LastAudit = auditEntry;
            return Task.FromResult(organization);
        }

        public Task<OrganizationMembership> SaveOrganizationMembershipAsync(OrganizationMembership membership, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default)
        {
            SavedOrganizationMembership = membership;
            LastAudit = auditEntry;
            return Task.FromResult(membership);
        }

        public Task TransferOwnershipAsync(Guid organizationId, Guid currentOwnerUserId, Guid newOwnerUserId, DateTime occurredUtc, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default)
        {
            LastAudit = auditEntry;
            return Task.CompletedTask;
        }

        public Task AttachVenueAsync(Guid organizationId, Guid venueId, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default)
        {
            LastAudit = auditEntry;
            return Task.CompletedTask;
        }

        public Task<VenueMembership> SaveVenueMembershipAsync(VenueMembership membership, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default)
        {
            LastAudit = auditEntry;
            return Task.FromResult(membership);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

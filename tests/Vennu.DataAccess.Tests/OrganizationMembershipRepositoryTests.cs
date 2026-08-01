using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class OrganizationMembershipRepositoryTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VenueId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid UserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task GetVenueMembershipAsync_UsesOrganizationVenueAndUserScope()
    {
        string? sql = null;
        object? parameters = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, capturedParameters) =>
            {
                sql = capturedSql;
                parameters = capturedParameters;
                return [];
            }
        };

        var result = await new OrganizationMembershipRepository(data)
            .GetVenueMembershipAsync(OrganizationId, VenueId, UserId);

        Assert.Null(result);
        Assert.Contains("OrganizationId = @OrganizationId AND VenueId = @VenueId AND UserId = @UserId", sql, StringComparison.Ordinal);
        Assert.Equal(OrganizationId, Property<Guid>(parameters!, "OrganizationId"));
        Assert.Equal(VenueId, Property<Guid>(parameters!, "VenueId"));
        Assert.Equal(UserId, Property<Guid>(parameters!, "UserId"));
    }

    [Fact]
    public async Task SaveOrganizationMembershipAsync_WritesMembershipAndAuditInOneTransaction()
    {
        string? sql = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, _) =>
            {
                sql = capturedSql;
                return [];
            }
        };
        var membership = OrganizationMember();

        var saved = await new OrganizationMembershipRepository(data)
            .SaveOrganizationMembershipAsync(membership, Audit(null));

        Assert.Same(membership, saved);
        Assert.Contains("BEGIN TRANSACTION", sql, StringComparison.Ordinal);
        Assert.Contains("MERGE dbo.OrganizationMemberships", sql, StringComparison.Ordinal);
        Assert.Contains("INSERT dbo.MembershipAuditEntries", sql, StringComparison.Ordinal);
        Assert.Contains("COMMIT", sql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TransferOwnershipAsync_UsesSerializableOwnerGuardAndImmutableAudit()
    {
        string? sql = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, _) =>
            {
                sql = capturedSql;
                return [];
            }
        };
        var newOwnerId = Guid.NewGuid();
        var audit = Audit(null);
        audit.SubjectUserId = newOwnerId;
        audit.Action = MembershipAuditAction.OrganizationOwnershipTransferred;

        await new OrganizationMembershipRepository(data).TransferOwnershipAsync(
            OrganizationId, UserId, newOwnerId, audit.OccurredUtc, audit);

        Assert.Contains("ISOLATION LEVEL SERIALIZABLE", sql, StringComparison.Ordinal);
        Assert.Contains("OwnerUserId = @CurrentOwnerUserId", sql, StringComparison.Ordinal);
        Assert.Contains("UPDATE dbo.Organizations SET OwnerUserId", sql, StringComparison.Ordinal);
        Assert.Contains("INSERT dbo.MembershipAuditEntries", sql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveVenueMembershipAsync_RejectsMismatchedAuditScope()
    {
        var membership = new VenueMembership
        {
            Id = Guid.NewGuid(), OrganizationId = OrganizationId, VenueId = VenueId, UserId = UserId,
            Role = VenueMembershipRole.Editor, GrantedUtc = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new OrganizationMembershipRepository(new FakeSqlDataAccess())
                .SaveVenueMembershipAsync(membership, Audit(null)));
    }

    private static OrganizationMembership OrganizationMember() => new()
    {
        Id = Guid.NewGuid(), OrganizationId = OrganizationId, UserId = UserId,
        Role = OrganizationMembershipRole.Admin, JoinedUtc = DateTime.UtcNow
    };

    private static MembershipAuditEntry Audit(Guid? venueId) => new()
    {
        Id = Guid.NewGuid(), OrganizationId = OrganizationId, VenueId = venueId,
        ActorUserId = UserId, SubjectUserId = UserId,
        Scope = venueId is null ? MembershipAuditScope.Organization : MembershipAuditScope.Venue,
        Action = MembershipAuditAction.OrganizationMemberAdded,
        OccurredUtc = DateTime.UtcNow
    };

    private static T Property<T>(object value, string name) =>
        (T)value.GetType().GetProperty(name)!.GetValue(value)!;
}

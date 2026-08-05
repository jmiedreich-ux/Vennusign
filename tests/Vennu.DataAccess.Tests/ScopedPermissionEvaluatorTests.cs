using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public sealed class ScopedPermissionEvaluatorTests
{
    private static readonly DateTime UtcNow = new(2026, 8, 5, 20, 40, 0, DateTimeKind.Utc);
    private readonly ScopedPermissionEvaluator evaluator = new();
    private readonly Guid actorId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private readonly Guid organizationId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private readonly Guid venueId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private readonly Guid resourceId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    [Fact]
    public void OrganizationAssignment_InheritsDownToVenueAndResource()
    {
        var assignment = Assignment("content_manager", AuthorityScopeType.Organization, organizationId);
        var target = AuthorityTarget.At(
            new AuthorityScope(AuthorityScopeType.Resource, resourceId),
            new AuthorityScope(AuthorityScopeType.Venue, venueId),
            new AuthorityScope(AuthorityScopeType.Organization, organizationId));

        var result = evaluator.Evaluate(
            actorId,
            PermissionId.Parse("content.item.update"),
            target,
            [assignment],
            UtcNow);

        Assert.True(result.IsAllowed);
        Assert.Equal(assignment.Id, result.AssignmentId);
        Assert.Equal("content_manager", result.RoleKey);
    }

    [Fact]
    public void VenueAssignment_DoesNotInheritUpToOrganization()
    {
        var result = evaluator.Evaluate(
            actorId,
            PermissionId.Parse("content.item.update"),
            AuthorityTarget.At(new AuthorityScope(AuthorityScopeType.Organization, organizationId)),
            [Assignment("content_manager", AuthorityScopeType.Venue, venueId)],
            UtcNow);

        Assert.False(result.IsAllowed);
        Assert.Equal("permission.required", result.ReasonCode);
    }

    [Fact]
    public void ContentEditorCannotPublishButPublisherCan()
    {
        var target = AuthorityTarget.At(new AuthorityScope(AuthorityScopeType.Venue, venueId));
        var permission = PermissionId.Parse("publishing.release.publish");

        Assert.False(evaluator.Evaluate(
            actorId,
            permission,
            target,
            [Assignment("content_editor", AuthorityScopeType.Venue, venueId)],
            UtcNow).IsAllowed);
        Assert.True(evaluator.Evaluate(
            actorId,
            permission,
            target,
            [Assignment("publisher", AuthorityScopeType.Venue, venueId)],
            UtcNow).IsAllowed);
    }

    [Fact]
    public void ExpiredFutureAndRevokedAssignmentsAreIgnored()
    {
        var expired = Assignment("organization_owner", AuthorityScopeType.Organization, organizationId) with
        {
            ExpiresUtc = UtcNow
        };
        var future = Assignment("organization_owner", AuthorityScopeType.Organization, organizationId) with
        {
            StartsUtc = UtcNow.AddMinutes(1)
        };
        var revoked = Assignment("organization_owner", AuthorityScopeType.Organization, organizationId) with
        {
            RevokedUtc = UtcNow.AddMinutes(-1)
        };

        var result = evaluator.Evaluate(
            actorId,
            PermissionId.Parse("account.member.manage"),
            AuthorityTarget.At(new AuthorityScope(AuthorityScopeType.Organization, organizationId)),
            [expired, future, revoked],
            UtcNow);

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void SelfScopeCannotAuthorizeAnotherActor()
    {
        var assignment = Assignment("viewer", AuthorityScopeType.Self, actorId);

        var result = evaluator.Evaluate(
            actorId,
            PermissionId.Parse("account.billing.view"),
            AuthorityTarget.At(new AuthorityScope(AuthorityScopeType.Self, Guid.NewGuid())),
            [assignment],
            UtcNow);

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void PermissionDecisionDimensionPreservesPermissionAndScope()
    {
        var authority = evaluator.Evaluate(
            actorId,
            PermissionId.Parse("publishing.release.publish"),
            AuthorityTarget.At(new AuthorityScope(AuthorityScopeType.Venue, venueId)),
            [Assignment("content_editor", AuthorityScopeType.Venue, venueId)],
            UtcNow);

        var dimension = ScopedPermissionDecisionDimensionFactory.Create(authority);

        Assert.Equal(CapabilityDecisionCategory.Permission, dimension.Category);
        Assert.Equal(CapabilityDecisionOutcome.Denied, dimension.FailureOutcome);
        Assert.Equal("publishing.release.publish", dimension.Parameters!["permission"]);
        Assert.Equal("Venue", dimension.Parameters["scopeType"]);
    }

    [Fact]
    public void SystemRolesAreProtectedAndSupportIsOutsideCustomerRoles()
    {
        Assert.All(SystemRoleRegistry.Roles.Values, role => Assert.True(role.IsProtected));
        Assert.DoesNotContain(
            SystemRoleRegistry.Roles["organization_owner"].Permissions,
            permission => permission.Value.StartsWith("support.", StringComparison.Ordinal));
        Assert.All(
            SystemRoleRegistry.Roles["support_operator"].Permissions,
            permission => Assert.StartsWith("support.", permission.Value, StringComparison.Ordinal));
    }

    private ScopedRoleAssignment Assignment(string role, AuthorityScopeType scopeType, Guid scopeId) =>
        new()
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorId,
            RoleKey = role,
            ScopeType = scopeType,
            ScopeId = scopeId,
            StartsUtc = UtcNow.AddHours(-1),
            CreatedByUserId = Guid.NewGuid(),
            CreatedUtc = UtcNow.AddHours(-1)
        };
}

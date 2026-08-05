using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IScopedPermissionEvaluator
{
    ScopedAuthorityResult Evaluate(
        Guid actorUserId,
        PermissionId permission,
        AuthorityTarget target,
        IReadOnlyCollection<ScopedRoleAssignment> assignments,
        DateTime utcNow);
}

public sealed class ScopedPermissionEvaluator : IScopedPermissionEvaluator
{
    public ScopedAuthorityResult Evaluate(
        Guid actorUserId,
        PermissionId permission,
        AuthorityTarget target,
        IReadOnlyCollection<ScopedRoleAssignment> assignments,
        DateTime utcNow)
    {
        if (actorUserId == Guid.Empty) throw new ArgumentException("Actor user ID is required.", nameof(actorUserId));
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(assignments);

        var active = assignments
            .Where(item => item.ActorUserId == actorUserId
                && item.StartsUtc <= utcNow
                && (item.ExpiresUtc is null || item.ExpiresUtc > utcNow)
                && item.RevokedUtc is null)
            .OrderByDescending(item => ScopeSpecificity(item.ScopeType))
            .ThenBy(item => item.Id)
            .ToArray();

        foreach (var assignment in active)
        {
            if (!SystemRoleRegistry.Roles.TryGetValue(assignment.RoleKey, out var role)
                || !role.Permissions.Contains(permission)
                || !Applies(assignment, actorUserId, target))
            {
                continue;
            }

            return new ScopedAuthorityResult(
                ScopedAuthorityOutcome.Allowed,
                permission,
                target.Target,
                "permission.allowed",
                "decisions.permission.allowed",
                assignment.Id,
                role.Key);
        }

        return new ScopedAuthorityResult(
            ScopedAuthorityOutcome.Denied,
            permission,
            target.Target,
            "permission.required",
            "decisions.permission.required");
    }

    private static bool Applies(ScopedRoleAssignment assignment, Guid actorUserId, AuthorityTarget target)
    {
        if (assignment.ScopeType == AuthorityScopeType.Self)
        {
            return assignment.ScopeId == actorUserId
                && target.Target.Type == AuthorityScopeType.Self
                && target.Target.Id == actorUserId;
        }

        var scope = new AuthorityScope(assignment.ScopeType, assignment.ScopeId);
        if (scope == target.Target || target.Ancestors.Contains(scope)) return true;
        if (assignment.ScopeType == AuthorityScopeType.Platform) return true;
        return false;
    }

    private static int ScopeSpecificity(AuthorityScopeType type) => type switch
    {
        AuthorityScopeType.Resource => 6,
        AuthorityScopeType.Self => 6,
        AuthorityScopeType.Venue => 5,
        AuthorityScopeType.VenueGroup => 4,
        AuthorityScopeType.Organization => 3,
        AuthorityScopeType.Platform => 1,
        _ => 0
    };
}

public static class ScopedPermissionDecisionDimensionFactory
{
    public static CapabilityDecisionDimension Create(ScopedAuthorityResult authority) =>
        authority.IsAllowed
            ? CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.Permission)
            : CapabilityDecisionDimension.Failed(
                CapabilityDecisionCategory.Permission,
                CapabilityDecisionOutcome.Denied,
                authority.ReasonCode,
                authority.MessageKey,
                new Dictionary<string, string>
                {
                    ["permission"] = authority.Permission.Value,
                    ["scopeType"] = authority.Target.Type.ToString(),
                    ["scopeId"] = authority.Target.Id.ToString("D")
                },
                "ask_scope_administrator");
}

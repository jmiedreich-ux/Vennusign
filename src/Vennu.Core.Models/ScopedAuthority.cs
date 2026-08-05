using System.Collections.ObjectModel;

namespace Vennu.Core.Models;

public readonly record struct PermissionId
{
    private PermissionId(string value) => Value = value;

    public string Value { get; }

    public static PermissionId Parse(string? value)
    {
        var capability = CapabilityId.Parse(value);
        return new PermissionId(capability.Value);
    }

    public override string ToString() => Value ?? string.Empty;
}

public enum AuthorityScopeType
{
    Platform = 1,
    Organization = 2,
    VenueGroup = 3,
    Venue = 4,
    Resource = 5,
    Self = 6
}

public sealed record AuthorityScope(AuthorityScopeType Type, Guid Id);

public sealed record AuthorityTarget(AuthorityScope Target, IReadOnlyCollection<AuthorityScope> Ancestors)
{
    public static AuthorityTarget At(AuthorityScope target, params AuthorityScope[] ancestors) =>
        new(target, Array.AsReadOnly(ancestors));
}

public sealed record SystemRoleDefinition(
    string Key,
    string NameMessageKey,
    bool IsProtected,
    IReadOnlyCollection<PermissionId> Permissions);

public sealed record ScopedRoleAssignment
{
    public Guid Id { get; set; }
    public Guid ActorUserId { get; set; }
    public string RoleKey { get; set; } = string.Empty;
    public AuthorityScopeType ScopeType { get; set; }
    public Guid ScopeId { get; set; }
    public DateTime StartsUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public enum ScopedAuthorityOutcome
{
    Allowed = 1,
    Denied = 2
}

public sealed record ScopedAuthorityResult(
    ScopedAuthorityOutcome Outcome,
    PermissionId Permission,
    AuthorityScope Target,
    string ReasonCode,
    string MessageKey,
    Guid? AssignmentId = null,
    string? RoleKey = null)
{
    public bool IsAllowed => Outcome == ScopedAuthorityOutcome.Allowed;
}

public sealed class SupportAccessGrant
{
    public Guid Id { get; set; }
    public Guid SupportUserId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? VenueId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime StartsUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public enum SupportAccessAuditAction
{
    Entered = 1,
    Exited = 2,
    Denied = 3,
    Expired = 4,
    Revoked = 5
}

public sealed class SupportAccessAuditEntry
{
    public Guid Id { get; set; }
    public Guid? GrantId { get; set; }
    public Guid ActorUserId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? VenueId { get; set; }
    public SupportAccessAuditAction Action { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime OccurredUtc { get; set; }
}

public sealed record SupportAccessContext(
    Guid GrantId,
    Guid SupportUserId,
    Guid OrganizationId,
    Guid? VenueId,
    string Reason,
    DateTime ExpiresUtc,
    bool RequiresProminentIndicator = true);

public static class PermissionRegistry
{
    private static readonly ReadOnlyDictionary<PermissionId, CapabilityId> CapabilityByPermissionValue =
        new(Version1CapabilityRegistry.Definitions.ToDictionary(
            definition => PermissionId.Parse(definition.Id.Value),
            definition => definition.Id));

    public static IReadOnlyDictionary<PermissionId, CapabilityId> CapabilityByPermission => CapabilityByPermissionValue;

    public static PermissionId For(CapabilityId capability) => PermissionId.Parse(capability.Value);
}

public static class SystemRoleRegistry
{
    private static readonly PermissionId[] AllCustomerPermissions = Version1CapabilityRegistry.Definitions
        .Where(item => item.Domain != CapabilityDomain.Support)
        .Select(item => PermissionRegistry.For(item.Id))
        .ToArray();

    private static readonly ReadOnlyDictionary<string, SystemRoleDefinition> RolesValue = Create(
    [
        Role("organization_owner", AllCustomerPermissions),
        Role("organization_administrator", AllCustomerPermissions.Where(permission => permission.Value != "account.security.manage")),
        Role("venue_administrator", Prefixes("content.", "publishing.", "screen.", "schedule.", "localization.", "analytics.delivery_health.", "branding.theme.")),
        Role("content_manager", Prefixes("content.", "publishing.", "schedule.", "localization.", "branding.")),
        Role("content_editor", ExactAndPrefixes(
            ["publishing.release.preview", "analytics.delivery_health.view"],
            "content.", "localization.variant.", "branding.theme.")),
        Role("publisher", ExactAndPrefixes(
            [
                "publishing.release.preview", "publishing.release.publish", "publishing.release.confirm",
                "publishing.release.replace", "publishing.release.unpublish", "publishing.delivery.retry",
                "publishing.delivery.restore", "screen.device.view", "screen.delivery.view", "screen.delivery.recover"
            ])),
        Role("viewer", ExactAndPrefixes(
            ["publishing.release.preview", "screen.device.view", "screen.delivery.view", "analytics.delivery_health.view", "account.billing.view"])),
        Role("support_operator", Prefixes("support."))
    ]);

    public static IReadOnlyDictionary<string, SystemRoleDefinition> Roles => RolesValue;

    private static SystemRoleDefinition Role(string key, IEnumerable<PermissionId> permissions) =>
        new(key, $"roles.{key}.name", true, Array.AsReadOnly(permissions.Distinct().OrderBy(item => item.Value).ToArray()));

    private static IEnumerable<PermissionId> Prefixes(params string[] prefixes) =>
        PermissionRegistry.CapabilityByPermission.Keys.Where(permission => prefixes.Any(prefix => permission.Value.StartsWith(prefix, StringComparison.Ordinal)));

    private static IEnumerable<PermissionId> ExactAndPrefixes(IEnumerable<string> exact, params string[] prefixes)
    {
        var exactValues = exact.ToHashSet(StringComparer.Ordinal);
        return PermissionRegistry.CapabilityByPermission.Keys.Where(permission =>
            exactValues.Contains(permission.Value) || prefixes.Any(prefix => permission.Value.StartsWith(prefix, StringComparison.Ordinal)));
    }

    private static ReadOnlyDictionary<string, SystemRoleDefinition> Create(IEnumerable<SystemRoleDefinition> roles) =>
        new(roles.ToDictionary(role => role.Key, StringComparer.Ordinal));
}

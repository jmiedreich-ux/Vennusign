using System.Security.Claims;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.BackOffice;

public sealed class BackOfficeCapabilityDecisionInputProvider(
    IHttpContextAccessor httpContextAccessor,
    ICapabilityAccessPolicyRepository accessPolicies,
    IScopedAuthorityRepository scopedAuthority,
    IScopedPermissionEvaluator permissionEvaluator,
    TimeProvider timeProvider) : ICapabilityDecisionInputProvider
{
    public async Task<CapabilityDecisionInput> ResolveAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        var principal = context?.User;
        var userId = ParseGuid(principal?.FindFirstValue(ClaimTypes.NameIdentifier));
        var venueId = ParseGuid(principal?.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim));
        var organizationId = ParseGuid(principal?.FindFirstValue(BackOfficeAuthenticationDefaults.OrganizationIdClaim));
        var definition = Version1CapabilityRegistry.Get(capability);
        var policy = organizationId is Guid organization && venueId is Guid venue
            ? await accessPolicies.GetAsync(organization, venue, capability, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false)
            : CapabilityAccessPolicy.DefaultFor(definition);
        var hasPermission = userId is Guid actor && organizationId is Guid organizationScope && venueId is Guid venueScope
            && await HasPermissionAsync(
                principal,
                actor,
                organizationScope,
                venueScope,
                capability,
                timeProvider.GetUtcNow().UtcDateTime,
                cancellationToken).ConfigureAwait(false);

        var dimensions = new List<CapabilityDecisionDimension>
        {
            userId is not null && venueId is not null && organizationId is not null
                ? CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.IdentityContext)
                : CapabilityDecisionDimension.Failed(
                    CapabilityDecisionCategory.IdentityContext,
                    CapabilityDecisionOutcome.Denied,
                    "identity.context_required",
                    "decisions.identity.required",
                    resolution: "sign_in_again"),
            Rollout(policy, timeProvider.GetUtcNow().UtcDateTime),
            policy.Entitled
                ? CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.Entitlement)
                : CapabilityDecisionDimension.Failed(
                    CapabilityDecisionCategory.Entitlement,
                    CapabilityDecisionOutcome.Unavailable,
                    "entitlement.not_included",
                    "decisions.entitlement.required",
                    resolution: "review_product_access"),
            hasPermission
                ? CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.Permission)
                : CapabilityDecisionDimension.Failed(
                    CapabilityDecisionCategory.Permission,
                    CapabilityDecisionOutcome.Denied,
                    "permission.required",
                    "decisions.permission.required",
                    resolution: "ask_scope_administrator"),
            policy.AddOnAttached
                ? CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.AddOn)
                : CapabilityDecisionDimension.Failed(
                    CapabilityDecisionCategory.AddOn,
                    CapabilityDecisionOutcome.Unavailable,
                    "add_on.not_attached",
                    "decisions.add_on.required",
                    resolution: "connect_required_service"),
            policy.AllowanceAvailable || IsCorrectionOrRecovery(capability)
                ? CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.Allowance)
                : CapabilityDecisionDimension.Failed(
                    CapabilityDecisionCategory.Allowance,
                    CapabilityDecisionOutcome.Denied,
                    "allowance.reached",
                    "decisions.allowance.reached",
                    new Dictionary<string, string>
                    {
                        ["limit"] = policy.AllowanceLimit?.ToString() ?? string.Empty,
                        ["used"] = policy.AllowanceUsed.ToString()
                    },
                    "remove_or_increase_allowance"),
            CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.ResourceState),
            CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.RequestValidity)
        };

        return new CapabilityDecisionInput(capability, dimensions, correlationId, locale);
    }

    public async Task<IReadOnlyCollection<CapabilityDecisionInput>> ResolveBatchAsync(
        IReadOnlyCollection<CapabilityId> capabilities,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        var resolved = new List<CapabilityDecisionInput>(capabilities.Count);
        foreach (var capability in capabilities.Distinct())
            resolved.Add(await ResolveAsync(capability, correlationId, locale, cancellationToken).ConfigureAwait(false));
        return resolved.AsReadOnly();
    }

    private static CapabilityDecisionDimension Rollout(CapabilityAccessPolicy policy, DateTime utcNow) => policy.Rollout switch
    {
        CapabilityRolloutState.Available => CapabilityDecisionDimension.Satisfied(CapabilityDecisionCategory.Rollout),
        CapabilityRolloutState.TemporarilyBlocked => CapabilityDecisionDimension.Failed(
            CapabilityDecisionCategory.Rollout,
            CapabilityDecisionOutcome.TemporarilyBlocked,
            "rollout.temporarily_blocked",
            "decisions.rollout.temporary",
            resolution: "retry_later",
            retryAfter: policy.RetryAfterUtc is DateTime retry && retry > utcNow ? retry - utcNow : null),
        _ => CapabilityDecisionDimension.Failed(
            CapabilityDecisionCategory.Rollout,
            CapabilityDecisionOutcome.Unavailable,
            "rollout.unavailable",
            "decisions.rollout.unavailable")
    };

    private async Task<bool> HasPermissionAsync(
        ClaimsPrincipal? principal,
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        CapabilityId capability,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var assignments = (await scopedAuthority.GetActiveAssignmentsAsync(
            actorUserId,
            utcNow,
            cancellationToken).ConfigureAwait(false)).ToList();
        assignments.AddRange(principal?.FindAll(BackOfficeAuthenticationDefaults.SystemRoleClaim)
            .Select(claim => claim.Value)
            .Where(SystemRoleRegistry.Roles.ContainsKey)
            .Select(roleKey => new ScopedRoleAssignment
            {
                Id = DeterministicAssignmentId(actorUserId, organizationId, venueId, roleKey),
                ActorUserId = actorUserId,
                RoleKey = roleKey,
                ScopeType = roleKey.StartsWith("organization_", StringComparison.Ordinal)
                    ? AuthorityScopeType.Organization
                    : AuthorityScopeType.Venue,
                ScopeId = roleKey.StartsWith("organization_", StringComparison.Ordinal) ? organizationId : venueId,
                StartsUtc = DateTime.MinValue,
                CreatedByUserId = actorUserId,
                CreatedUtc = DateTime.MinValue
            }) ?? []);

        var target = AuthorityTarget.At(
            new AuthorityScope(AuthorityScopeType.Venue, venueId),
            new AuthorityScope(AuthorityScopeType.Organization, organizationId));
        return permissionEvaluator.Evaluate(
            actorUserId,
            PermissionRegistry.For(capability),
            target,
            assignments,
            utcNow).IsAllowed;
    }

    private static Guid DeterministicAssignmentId(Guid actorUserId, Guid organizationId, Guid venueId, string roleKey)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{actorUserId:D}|{organizationId:D}|{venueId:D}|{roleKey}"));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static bool IsCorrectionOrRecovery(CapabilityId capability) => capability.Value is
        "content.item.update" or "content.item.archive" or "publishing.release.replace" or
        "publishing.release.unpublish" or "publishing.delivery.retry" or
        "publishing.delivery.restore" or "screen.device.unpair" or "screen.delivery.recover";

    private static Guid? ParseGuid(string? value) => Guid.TryParse(value, out var parsed) && parsed != Guid.Empty ? parsed : null;
}

using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public interface ISupportAccessService
{
    Task<SupportAccessContext> EnterAsync(
        Guid supportUserId,
        Guid organizationId,
        Guid? venueId,
        string correlationId,
        CancellationToken cancellationToken = default);
}

public sealed class SupportAccessService(
    IScopedAuthorityRepository repository,
    IScopedPermissionEvaluator permissions,
    TimeProvider timeProvider) : ISupportAccessService
{
    public async Task<SupportAccessContext> EnterAsync(
        Guid supportUserId,
        Guid organizationId,
        Guid? venueId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var assignments = await repository.GetActiveAssignmentsAsync(supportUserId, utcNow, cancellationToken).ConfigureAwait(false);
        var platform = new AuthorityScope(AuthorityScopeType.Platform, AuthorityScopeIds.Platform);
        var authority = permissions.Evaluate(
            supportUserId,
            PermissionId.Parse("support.context.enter"),
            AuthorityTarget.At(platform),
            assignments,
            utcNow);
        var grant = authority.IsAllowed
            ? await repository.GetActiveSupportGrantAsync(
                supportUserId,
                organizationId,
                venueId,
                utcNow,
                cancellationToken).ConfigureAwait(false)
            : null;

        if (!authority.IsAllowed || grant is null || string.IsNullOrWhiteSpace(grant.Reason))
        {
            await AuditAsync(null, SupportAccessAuditAction.Denied, "support_access_not_authorized").ConfigureAwait(false);
            throw new UnauthorizedAccessException("Explicit, active and reasoned support access is required.");
        }

        await AuditAsync(grant, SupportAccessAuditAction.Entered, grant.Reason).ConfigureAwait(false);
        return new SupportAccessContext(
            grant.Id,
            supportUserId,
            organizationId,
            venueId,
            grant.Reason,
            grant.ExpiresUtc);

        Task AuditAsync(SupportAccessGrant? activeGrant, SupportAccessAuditAction action, string reason) =>
            repository.AppendSupportAuditAsync(
                new SupportAccessAuditEntry
                {
                    Id = Guid.NewGuid(),
                    GrantId = activeGrant?.Id,
                    ActorUserId = supportUserId,
                    OrganizationId = organizationId,
                    VenueId = venueId,
                    Action = action,
                    Reason = reason,
                    CorrelationId = correlationId,
                    OccurredUtc = utcNow
                },
                cancellationToken);
    }
}

public static class AuthorityScopeIds
{
    public static readonly Guid Platform = Guid.Parse("01000000-0000-0000-0000-000000000001");
}

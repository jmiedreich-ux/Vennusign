using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class ScopedAuthorityRepository(ISqlDataAccess dataAccess) : IScopedAuthorityRepository
{
    public async Task<IReadOnlyCollection<ScopedRoleAssignment>> GetActiveAssignmentsAsync(
        Guid actorUserId,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScopedRoleAssignment, object>(
            """
            SELECT Id, ActorUserId, RoleKey, ScopeType, ScopeId, StartsUtc, ExpiresUtc,
                RevokedUtc, CreatedByUserId, CreatedUtc
            FROM dbo.ScopedRoleAssignments
            WHERE ActorUserId = @ActorUserId
              AND StartsUtc <= @UtcNow
              AND (ExpiresUtc IS NULL OR ExpiresUtc > @UtcNow)
              AND RevokedUtc IS NULL;
            """,
            new { ActorUserId = Require(actorUserId, nameof(actorUserId)), UtcNow = utcNow },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task SaveAssignmentAsync(ScopedRoleAssignment assignment, CancellationToken cancellationToken = default)
    {
        ValidateAssignment(assignment);
        await dataAccess.MergeAllAsync([assignment], "dbo.ScopedRoleAssignments", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<SupportAccessGrant?> GetActiveSupportGrantAsync(
        Guid supportUserId,
        Guid organizationId,
        Guid? venueId,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<SupportAccessGrant, object>(
            """
            SELECT TOP (1) Id, SupportUserId, OrganizationId, VenueId, Reason, StartsUtc,
                ExpiresUtc, ApprovedByUserId, RevokedUtc, CreatedUtc
            FROM dbo.SupportAccessGrants
            WHERE SupportUserId = @SupportUserId
              AND OrganizationId = @OrganizationId
              AND (VenueId IS NULL OR VenueId = @VenueId)
              AND StartsUtc <= @UtcNow AND ExpiresUtc > @UtcNow AND RevokedUtc IS NULL
            ORDER BY CASE WHEN VenueId = @VenueId THEN 0 ELSE 1 END, ExpiresUtc;
            """,
            new
            {
                SupportUserId = Require(supportUserId, nameof(supportUserId)),
                OrganizationId = Require(organizationId, nameof(organizationId)),
                VenueId = venueId,
                UtcNow = utcNow
            },
            cancellationToken).ConfigureAwait(false)).FirstOrDefault();

    public async Task SaveSupportGrantAsync(SupportAccessGrant grant, CancellationToken cancellationToken = default)
    {
        ValidateGrant(grant);
        await dataAccess.MergeAllAsync([grant], "dbo.SupportAccessGrants", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task AppendSupportAuditAsync(SupportAccessAuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Require(entry.Id, nameof(entry.Id));
        Require(entry.ActorUserId, nameof(entry.ActorUserId));
        Require(entry.OrganizationId, nameof(entry.OrganizationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(entry.Reason);
        ArgumentException.ThrowIfNullOrWhiteSpace(entry.CorrelationId);
        await dataAccess.MergeAllAsync([entry], "dbo.SupportAccessAuditEntries", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateAssignment(ScopedRoleAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        Require(assignment.Id, nameof(assignment.Id));
        Require(assignment.ActorUserId, nameof(assignment.ActorUserId));
        Require(assignment.ScopeId, nameof(assignment.ScopeId));
        Require(assignment.CreatedByUserId, nameof(assignment.CreatedByUserId));
        if (!SystemRoleRegistry.Roles.ContainsKey(assignment.RoleKey)) throw new ArgumentException("Role key is not registered.", nameof(assignment));
        if (assignment.ExpiresUtc <= assignment.StartsUtc) throw new ArgumentException("Assignment expiry must follow its start.", nameof(assignment));
    }

    private static void ValidateGrant(SupportAccessGrant grant)
    {
        ArgumentNullException.ThrowIfNull(grant);
        Require(grant.Id, nameof(grant.Id));
        Require(grant.SupportUserId, nameof(grant.SupportUserId));
        Require(grant.OrganizationId, nameof(grant.OrganizationId));
        Require(grant.ApprovedByUserId, nameof(grant.ApprovedByUserId));
        ArgumentException.ThrowIfNullOrWhiteSpace(grant.Reason);
        if (grant.ExpiresUtc <= grant.StartsUtc) throw new ArgumentException("Support access expiry must follow its start.", nameof(grant));
        if (grant.ExpiresUtc - grant.StartsUtc > TimeSpan.FromHours(8)) throw new ArgumentException("Support access may not exceed eight hours.", nameof(grant));
    }

    private static Guid Require(Guid value, string name) =>
        value == Guid.Empty ? throw new ArgumentException("A non-empty ID is required.", name) : value;
}

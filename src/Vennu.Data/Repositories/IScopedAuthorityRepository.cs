using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IScopedAuthorityRepository
{
    Task<IReadOnlyCollection<ScopedRoleAssignment>> GetActiveAssignmentsAsync(
        Guid actorUserId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task SaveAssignmentAsync(ScopedRoleAssignment assignment, CancellationToken cancellationToken = default);

    Task<SupportAccessGrant?> GetActiveSupportGrantAsync(
        Guid supportUserId,
        Guid organizationId,
        Guid? venueId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task SaveSupportGrantAsync(SupportAccessGrant grant, CancellationToken cancellationToken = default);

    Task AppendSupportAuditAsync(SupportAccessAuditEntry entry, CancellationToken cancellationToken = default);
}

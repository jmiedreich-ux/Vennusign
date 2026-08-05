using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public sealed class SupportAccessServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 5, 20, 40, 0, TimeSpan.Zero);
    private readonly Guid supportUserId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    private readonly Guid organizationId = Guid.Parse("60000000-0000-0000-0000-000000000001");
    private readonly Guid venueId = Guid.Parse("70000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task EnterRequiresPlatformSupportRoleAndActiveReasonedGrant()
    {
        var repository = new FakeRepository
        {
            Assignments = [SupportAssignment()],
            Grant = ActiveGrant()
        };
        var service = Create(repository);

        var context = await service.EnterAsync(
            supportUserId,
            organizationId,
            venueId,
            "support-correlation");

        Assert.True(context.RequiresProminentIndicator);
        Assert.Equal("Investigate delivery incident INC-42", context.Reason);
        var audit = Assert.Single(repository.Audits);
        Assert.Equal(SupportAccessAuditAction.Entered, audit.Action);
        Assert.Equal(context.GrantId, audit.GrantId);
        Assert.Equal("support-correlation", audit.CorrelationId);
    }

    [Fact]
    public async Task MissingSupportRoleFailsAndIsAudited()
    {
        var repository = new FakeRepository { Grant = ActiveGrant() };
        var service = Create(repository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.EnterAsync(
            supportUserId,
            organizationId,
            venueId,
            "denied-correlation"));

        var audit = Assert.Single(repository.Audits);
        Assert.Equal(SupportAccessAuditAction.Denied, audit.Action);
        Assert.Null(audit.GrantId);
    }

    [Fact]
    public async Task MissingOrExpiredGrantFailsEvenWithSupportRole()
    {
        var repository = new FakeRepository { Assignments = [SupportAssignment()] };
        var service = Create(repository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.EnterAsync(
            supportUserId,
            organizationId,
            venueId,
            "expired-correlation"));

        Assert.Equal(SupportAccessAuditAction.Denied, Assert.Single(repository.Audits).Action);
    }

    private SupportAccessService Create(FakeRepository repository) =>
        new(repository, new ScopedPermissionEvaluator(), new FixedTimeProvider(UtcNow));

    private ScopedRoleAssignment SupportAssignment() => new()
    {
        Id = Guid.NewGuid(),
        ActorUserId = supportUserId,
        RoleKey = "support_operator",
        ScopeType = AuthorityScopeType.Platform,
        ScopeId = AuthorityScopeIds.Platform,
        StartsUtc = UtcNow.UtcDateTime.AddHours(-1),
        CreatedByUserId = Guid.NewGuid(),
        CreatedUtc = UtcNow.UtcDateTime.AddHours(-1)
    };

    private SupportAccessGrant ActiveGrant() => new()
    {
        Id = Guid.NewGuid(),
        SupportUserId = supportUserId,
        OrganizationId = organizationId,
        VenueId = venueId,
        Reason = "Investigate delivery incident INC-42",
        StartsUtc = UtcNow.UtcDateTime.AddMinutes(-10),
        ExpiresUtc = UtcNow.UtcDateTime.AddMinutes(50),
        ApprovedByUserId = Guid.NewGuid(),
        CreatedUtc = UtcNow.UtcDateTime.AddMinutes(-10)
    };

    private sealed class FakeRepository : IScopedAuthorityRepository
    {
        public IReadOnlyCollection<ScopedRoleAssignment> Assignments { get; init; } = [];
        public SupportAccessGrant? Grant { get; init; }
        public List<SupportAccessAuditEntry> Audits { get; } = [];

        public Task<IReadOnlyCollection<ScopedRoleAssignment>> GetActiveAssignmentsAsync(Guid actorUserId, DateTime utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult(Assignments);

        public Task SaveAssignmentAsync(ScopedRoleAssignment assignment, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<SupportAccessGrant?> GetActiveSupportGrantAsync(Guid supportUserId, Guid organizationId, Guid? venueId, DateTime utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult(Grant is { } grant && grant.StartsUtc <= utcNow && grant.ExpiresUtc > utcNow && grant.RevokedUtc is null ? grant : null);

        public Task SaveSupportGrantAsync(SupportAccessGrant grant, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task AppendSupportAuditAsync(SupportAccessAuditEntry entry, CancellationToken cancellationToken = default)
        {
            Audits.Add(entry);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

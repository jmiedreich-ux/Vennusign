using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class OrganizationMembershipRepository(ISqlDataAccess dataAccess)
    : IOrganizationMembershipRepository
{
    private const string OrganizationSql = """
        SELECT Id, Name, LegalName, PrimaryContactName, ContactEmail, ContactPhone, MailingAddress,
            OwnerUserId, CreatedUtc, UpdatedUtc
        FROM dbo.Organizations WHERE Id = @OrganizationId;
        """;

    public async Task<Organization?> GetOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<Organization, object>(OrganizationSql,
            new { OrganizationId = RequireId(organizationId, nameof(organizationId)) }, cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    private const string OrganizationMembershipSql = """
        SELECT Id, OrganizationId, UserId, Role, JoinedUtc, RevokedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.OrganizationMemberships
        WHERE OrganizationId = @OrganizationId AND UserId = @UserId;
        """;

    private const string VenueMembershipSql = """
        SELECT Id, OrganizationId, VenueId, UserId, Role, GrantedUtc, RevokedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.VenueMemberships
        WHERE OrganizationId = @OrganizationId AND VenueId = @VenueId AND UserId = @UserId;
        """;

    public async Task<OrganizationMembership?> GetOrganizationMembershipAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<OrganizationMembership, object>(
            OrganizationMembershipSql,
            new { OrganizationId = RequireId(organizationId, nameof(organizationId)), UserId = RequireId(userId, nameof(userId)) },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<VenueMembership?> GetVenueMembershipAsync(
        Guid organizationId,
        Guid venueId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<VenueMembership, object>(
            VenueMembershipSql,
            new
            {
                OrganizationId = RequireId(organizationId, nameof(organizationId)),
                VenueId = RequireId(venueId, nameof(venueId)),
                UserId = RequireId(userId, nameof(userId))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<Organization> CreateOrganizationAsync(
        Organization organization,
        OrganizationMembership ownerMembership,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default)
    {
        ValidateOrganizationMutation(organization, ownerMembership, auditEntry);
        var result = await dataAccess.ExecuteSqlQueryAsync<Organization, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            INSERT dbo.Organizations
                (Id, Name, LegalName, PrimaryContactName, ContactEmail, ContactPhone, MailingAddress, OwnerUserId, CreatedUtc, UpdatedUtc)
            VALUES
                (@OrganizationId, @Name, @LegalName, @PrimaryContactName, @ContactEmail, @ContactPhone, @MailingAddress, @OwnerUserId, @OccurredUtc, @OccurredUtc);
            INSERT dbo.OrganizationMemberships (Id, OrganizationId, UserId, Role, JoinedUtc, CreatedUtc, UpdatedUtc)
            VALUES (@MembershipId, @OrganizationId, @OwnerUserId, @OwnerRole, @OccurredUtc, @OccurredUtc, @OccurredUtc);
            INSERT dbo.MembershipAuditEntries
                (Id, OrganizationId, VenueId, ActorUserId, SubjectUserId, Scope, Action, PreviousRole, NewRole, OccurredUtc)
            VALUES (@AuditId, @OrganizationId, NULL, @OwnerUserId, @OwnerUserId, @Scope, @Action, NULL, @NewRole, @OccurredUtc);
            COMMIT;
            SELECT Id, Name, LegalName, PrimaryContactName, ContactEmail, ContactPhone, MailingAddress,
                OwnerUserId, CreatedUtc, UpdatedUtc FROM dbo.Organizations WHERE Id = @OrganizationId;
            """,
            new
            {
                OrganizationId = organization.Id,
                organization.Name,
                organization.LegalName,
                organization.PrimaryContactName,
                organization.ContactEmail,
                organization.ContactPhone,
                organization.MailingAddress,
                organization.OwnerUserId,
                MembershipId = ownerMembership.Id,
                OwnerRole = (int)ownerMembership.Role,
                AuditId = auditEntry.Id,
                Scope = (int)auditEntry.Scope,
                Action = (int)auditEntry.Action,
                auditEntry.NewRole,
                OccurredUtc = auditEntry.OccurredUtc
            },
            cancellationToken).ConfigureAwait(false);
        return result.SingleOrDefault() ?? organization;
    }

    public async Task<OrganizationMembership> SaveOrganizationMembershipAsync(
        OrganizationMembership membership,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default)
    {
        ValidateMembershipAudit(membership.OrganizationId, null, membership.UserId, auditEntry);
        var result = await dataAccess.ExecuteSqlQueryAsync<OrganizationMembership, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            MERGE dbo.OrganizationMemberships AS target
            USING (SELECT @OrganizationId OrganizationId, @UserId UserId) AS source
            ON target.OrganizationId = source.OrganizationId AND target.UserId = source.UserId
            WHEN MATCHED THEN UPDATE SET Role = @Role, RevokedUtc = @RevokedUtc, UpdatedUtc = @OccurredUtc
            WHEN NOT MATCHED THEN INSERT (Id, OrganizationId, UserId, Role, JoinedUtc, RevokedUtc, CreatedUtc, UpdatedUtc)
                VALUES (@MembershipId, @OrganizationId, @UserId, @Role, @JoinedUtc, @RevokedUtc, @OccurredUtc, @OccurredUtc);
            INSERT dbo.MembershipAuditEntries
                (Id, OrganizationId, VenueId, ActorUserId, SubjectUserId, Scope, Action, PreviousRole, NewRole, OccurredUtc)
            VALUES (@AuditId, @OrganizationId, NULL, @ActorUserId, @UserId, @Scope, @Action, @PreviousRole, @NewRole, @OccurredUtc);
            COMMIT;
            SELECT Id, OrganizationId, UserId, Role, JoinedUtc, RevokedUtc, CreatedUtc, UpdatedUtc
            FROM dbo.OrganizationMemberships WHERE OrganizationId = @OrganizationId AND UserId = @UserId;
            """,
            MutationParameters(membership, auditEntry),
            cancellationToken).ConfigureAwait(false);
        return result.SingleOrDefault() ?? membership;
    }

    public async Task TransferOwnershipAsync(
        Guid organizationId,
        Guid currentOwnerUserId,
        Guid newOwnerUserId,
        DateTime occurredUtc,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default)
    {
        RequireId(organizationId, nameof(organizationId));
        RequireId(currentOwnerUserId, nameof(currentOwnerUserId));
        RequireId(newOwnerUserId, nameof(newOwnerUserId));
        ValidateMembershipAudit(organizationId, null, newOwnerUserId, auditEntry);
        await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            """
            SET XACT_ABORT ON; SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; BEGIN TRANSACTION;
            IF NOT EXISTS (SELECT 1 FROM dbo.Organizations WITH (UPDLOCK, HOLDLOCK)
                WHERE Id = @OrganizationId AND OwnerUserId = @CurrentOwnerUserId)
                THROW 51001, 'Organization ownership changed before transfer.', 1;
            IF NOT EXISTS (SELECT 1 FROM dbo.OrganizationMemberships
                WHERE OrganizationId = @OrganizationId AND UserId = @NewOwnerUserId AND RevokedUtc IS NULL)
                THROW 51002, 'The new owner must be an active organization member.', 1;
            UPDATE dbo.OrganizationMemberships SET Role = @AdminRole, UpdatedUtc = @OccurredUtc
                WHERE OrganizationId = @OrganizationId AND UserId = @CurrentOwnerUserId;
            UPDATE dbo.OrganizationMemberships SET Role = @OwnerRole, UpdatedUtc = @OccurredUtc
                WHERE OrganizationId = @OrganizationId AND UserId = @NewOwnerUserId;
            UPDATE dbo.Organizations SET OwnerUserId = @NewOwnerUserId, UpdatedUtc = @OccurredUtc WHERE Id = @OrganizationId;
            INSERT dbo.MembershipAuditEntries
                (Id, OrganizationId, VenueId, ActorUserId, SubjectUserId, Scope, Action, PreviousRole, NewRole, OccurredUtc)
            VALUES (@AuditId, @OrganizationId, NULL, @CurrentOwnerUserId, @NewOwnerUserId, @Scope, @Action, @PreviousRole, @NewRole, @OccurredUtc);
            COMMIT; SELECT CAST(1 AS BIT) Applied;
            """,
            new
            {
                OrganizationId = organizationId,
                CurrentOwnerUserId = currentOwnerUserId,
                NewOwnerUserId = newOwnerUserId,
                AdminRole = (int)OrganizationMembershipRole.Admin,
                OwnerRole = (int)OrganizationMembershipRole.Owner,
                AuditId = auditEntry.Id,
                Scope = (int)auditEntry.Scope,
                Action = (int)auditEntry.Action,
                auditEntry.PreviousRole,
                auditEntry.NewRole,
                OccurredUtc = occurredUtc
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task AttachVenueAsync(
        Guid organizationId,
        Guid venueId,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default)
    {
        ValidateMembershipAudit(organizationId, venueId, auditEntry.SubjectUserId, auditEntry);
        await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            UPDATE dbo.Venues SET OrganizationId = @OrganizationId
            WHERE Id = @VenueId AND (OrganizationId IS NULL OR OrganizationId = @OrganizationId);
            IF @@ROWCOUNT = 0 THROW 51003, 'Venue is missing or belongs to another organization.', 1;
            INSERT dbo.MembershipAuditEntries
                (Id, OrganizationId, VenueId, ActorUserId, SubjectUserId, Scope, Action, PreviousRole, NewRole, OccurredUtc)
            VALUES (@AuditId, @OrganizationId, @VenueId, @ActorUserId, @SubjectUserId, @Scope, @Action, NULL, NULL, @OccurredUtc);
            COMMIT; SELECT CAST(1 AS BIT) Applied;
            """,
            AuditParameters(auditEntry),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<VenueMembership> SaveVenueMembershipAsync(
        VenueMembership membership,
        MembershipAuditEntry auditEntry,
        CancellationToken cancellationToken = default)
    {
        ValidateMembershipAudit(membership.OrganizationId, membership.VenueId, membership.UserId, auditEntry);
        var result = await dataAccess.ExecuteSqlQueryAsync<VenueMembership, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            MERGE dbo.VenueMemberships AS target
            USING (SELECT @VenueId VenueId, @UserId UserId) AS source
            ON target.VenueId = source.VenueId AND target.UserId = source.UserId
            WHEN MATCHED THEN UPDATE SET Role = @Role, RevokedUtc = @RevokedUtc, UpdatedUtc = @OccurredUtc
            WHEN NOT MATCHED THEN INSERT (Id, OrganizationId, VenueId, UserId, Role, GrantedUtc, RevokedUtc, CreatedUtc, UpdatedUtc)
                VALUES (@MembershipId, @OrganizationId, @VenueId, @UserId, @Role, @GrantedUtc, @RevokedUtc, @OccurredUtc, @OccurredUtc);
            INSERT dbo.MembershipAuditEntries
                (Id, OrganizationId, VenueId, ActorUserId, SubjectUserId, Scope, Action, PreviousRole, NewRole, OccurredUtc)
            VALUES (@AuditId, @OrganizationId, @VenueId, @ActorUserId, @UserId, @Scope, @Action, @PreviousRole, @NewRole, @OccurredUtc);
            COMMIT;
            SELECT Id, OrganizationId, VenueId, UserId, Role, GrantedUtc, RevokedUtc, CreatedUtc, UpdatedUtc
            FROM dbo.VenueMemberships WHERE VenueId = @VenueId AND UserId = @UserId;
            """,
            VenueMutationParameters(membership, auditEntry),
            cancellationToken).ConfigureAwait(false);
        return result.SingleOrDefault() ?? membership;
    }

    private static object MutationParameters(OrganizationMembership membership, MembershipAuditEntry audit) => new
    {
        MembershipId = membership.Id,
        membership.OrganizationId,
        membership.UserId,
        Role = (int)membership.Role,
        membership.JoinedUtc,
        membership.RevokedUtc,
        OccurredUtc = audit.OccurredUtc,
        AuditId = audit.Id,
        audit.ActorUserId,
        Scope = (int)audit.Scope,
        Action = (int)audit.Action,
        audit.PreviousRole,
        audit.NewRole
    };

    private static object VenueMutationParameters(VenueMembership membership, MembershipAuditEntry audit) => new
    {
        MembershipId = membership.Id,
        membership.OrganizationId,
        membership.VenueId,
        membership.UserId,
        Role = (int)membership.Role,
        membership.GrantedUtc,
        membership.RevokedUtc,
        OccurredUtc = audit.OccurredUtc,
        AuditId = audit.Id,
        audit.ActorUserId,
        Scope = (int)audit.Scope,
        Action = (int)audit.Action,
        audit.PreviousRole,
        audit.NewRole
    };

    private static object AuditParameters(MembershipAuditEntry audit) => new
    {
        AuditId = audit.Id,
        audit.OrganizationId,
        audit.VenueId,
        audit.ActorUserId,
        audit.SubjectUserId,
        Scope = (int)audit.Scope,
        Action = (int)audit.Action,
        audit.OccurredUtc
    };

    private static void ValidateOrganizationMutation(
        Organization organization,
        OrganizationMembership membership,
        MembershipAuditEntry audit)
    {
        ArgumentNullException.ThrowIfNull(organization);
        ArgumentNullException.ThrowIfNull(membership);
        RequireId(organization.Id, nameof(organization.Id));
        RequireId(organization.OwnerUserId, nameof(organization.OwnerUserId));
        if (membership.OrganizationId != organization.Id || membership.UserId != organization.OwnerUserId || membership.Role != OrganizationMembershipRole.Owner)
            throw new ArgumentException("The initial membership must be the organization's owner.", nameof(membership));
        ValidateMembershipAudit(organization.Id, null, organization.OwnerUserId, audit);
    }

    private static void ValidateMembershipAudit(Guid organizationId, Guid? venueId, Guid subjectUserId, MembershipAuditEntry audit)
    {
        ArgumentNullException.ThrowIfNull(audit);
        RequireId(organizationId, nameof(organizationId));
        RequireId(subjectUserId, nameof(subjectUserId));
        if (audit.OrganizationId != organizationId || audit.VenueId != venueId || audit.SubjectUserId != subjectUserId)
            throw new ArgumentException("The audit entry must match the membership mutation.", nameof(audit));
        RequireId(audit.Id, nameof(audit.Id));
        RequireId(audit.ActorUserId, nameof(audit.ActorUserId));
    }

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty identifier is required.", parameterName);

    private sealed class MutationResult { public bool Applied { get; set; } }
}

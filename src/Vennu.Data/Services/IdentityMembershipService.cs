using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class IdentityMembershipService(
    ICustomerIdentityRepository identityRepository,
    IOrganizationMembershipRepository membershipRepository,
    IMembershipCapabilityResolver capabilityResolver,
    TimeProvider timeProvider,
    IVenueEntitlementService? entitlementService = null,
    IOrganizationSubscriptionRepository? organizationSubscriptions = null,
    IOrganizationSubscriptionProjectionService? projectionService = null) : IIdentityMembershipService
{
    public async Task<Organization> CreateOrganizationAsync(
        string name,
        Guid ownerUserId,
        CancellationToken cancellationToken = default) =>
        await CreateOrganizationCoreAsync(new OrganizationProfile(name, null, string.Empty, string.Empty, null, string.Empty), ownerUserId, false, cancellationToken).ConfigureAwait(false);

    public async Task<Organization> CreateOrganizationAsync(
        OrganizationProfile profile,
        Guid ownerUserId,
        CancellationToken cancellationToken = default) =>
        await CreateOrganizationCoreAsync(profile, ownerUserId, true, cancellationToken).ConfigureAwait(false);

    private async Task<Organization> CreateOrganizationCoreAsync(
        OrganizationProfile profile,
        Guid ownerUserId,
        bool requireCompleteProfile,
        CancellationToken cancellationToken)
    {
        var owner = await identityRepository.GetUserByIdAsync(RequireId(ownerUserId, nameof(ownerUserId)), cancellationToken)
            .ConfigureAwait(false);
        if (owner is null || owner.Status != CustomerUserStatus.Active)
            throw new InvalidOperationException("An active customer user is required to own an organization.");

        ArgumentNullException.ThrowIfNull(profile);
        var normalizedName = Required(profile.Name, 200, nameof(profile.Name));
        var contactName = requireCompleteProfile ? Required(profile.PrimaryContactName, 200, nameof(profile.PrimaryContactName)) : null;
        var contactEmail = requireCompleteProfile ? Required(profile.ContactEmail, 320, nameof(profile.ContactEmail)) : null;
        if (requireCompleteProfile && (contactEmail!.IndexOf('@') <= 0 || contactEmail.LastIndexOf('.') < contactEmail.IndexOf('@')))
            throw new ArgumentException("Enter a valid contact email address.", nameof(profile.ContactEmail));
        var mailingAddress = requireCompleteProfile ? Required(profile.MailingAddress, 500, nameof(profile.MailingAddress)) : null;

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            LegalName = Optional(profile.LegalName, 200, nameof(profile.LegalName)),
            PrimaryContactName = contactName,
            ContactEmail = contactEmail?.ToLowerInvariant(),
            ContactPhone = Optional(profile.ContactPhone, 50, nameof(profile.ContactPhone)),
            MailingAddress = mailingAddress,
            OwnerUserId = ownerUserId,
            CreatedUtc = utcNow,
            UpdatedUtc = utcNow
        };
        var membership = new OrganizationMembership
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserId = ownerUserId,
            Role = OrganizationMembershipRole.Owner,
            JoinedUtc = utcNow,
            CreatedUtc = utcNow,
            UpdatedUtc = utcNow
        };
        return await membershipRepository.CreateOrganizationAsync(
            organization,
            membership,
            Audit(organization.Id, null, ownerUserId, ownerUserId, MembershipAuditAction.OrganizationCreated, null, OrganizationMembershipRole.Owner.ToString(), utcNow),
            cancellationToken).ConfigureAwait(false);
    }

    private static string Required(string? value, int maximumLength, string name)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) throw new ArgumentException("A value is required.", name);
        return normalized.Length <= maximumLength ? normalized : throw new ArgumentException($"The value cannot exceed {maximumLength} characters.", name);
    }

    private static string? Optional(string? value, int maximumLength, string name) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, maximumLength, name);

    public async Task<OrganizationMembership> AddOrChangeOrganizationMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid subjectUserId,
        OrganizationMembershipRole role,
        CancellationToken cancellationToken = default)
    {
        ValidateAssignableOrganizationRole(role);
        var actor = await RequireOrganizationCapabilityAsync(
            actorUserId, organizationId, MembershipCapability.ManageOrganizationMembers, cancellationToken).ConfigureAwait(false);
        var subject = await identityRepository.GetUserByIdAsync(RequireId(subjectUserId, nameof(subjectUserId)), cancellationToken)
            .ConfigureAwait(false);
        if (subject is null || subject.Status != CustomerUserStatus.Active)
            throw new InvalidOperationException("An active customer user is required for membership.");

        var existing = await membershipRepository.GetOrganizationMembershipAsync(organizationId, subjectUserId, cancellationToken)
            .ConfigureAwait(false);
        if (existing?.Role == OrganizationMembershipRole.Owner && existing.RevokedUtc is null)
            throw new InvalidOperationException("Ownership can only be changed through an ownership transfer.");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var membership = existing ?? new OrganizationMembership
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, UserId = subjectUserId, JoinedUtc = utcNow, CreatedUtc = utcNow
        };
        var previousRole = existing?.RevokedUtc is null ? existing?.Role.ToString() : null;
        membership.Role = role;
        membership.RevokedUtc = null;
        membership.UpdatedUtc = utcNow;
        var action = previousRole is null ? MembershipAuditAction.OrganizationMemberAdded : MembershipAuditAction.OrganizationMemberRoleChanged;
        return await membershipRepository.SaveOrganizationMembershipAsync(
            membership,
            Audit(organizationId, null, actor.UserId, subjectUserId, action, previousRole, role.ToString(), utcNow),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task RevokeOrganizationMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid subjectUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = await RequireOrganizationCapabilityAsync(
            actorUserId, organizationId, MembershipCapability.ManageOrganizationMembers, cancellationToken).ConfigureAwait(false);
        var membership = await RequireActiveOrganizationMembershipAsync(organizationId, subjectUserId, cancellationToken)
            .ConfigureAwait(false);
        if (membership.Role == OrganizationMembershipRole.Owner)
            throw new InvalidOperationException("The active owner cannot be revoked; transfer ownership first.");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        membership.RevokedUtc = utcNow;
        membership.UpdatedUtc = utcNow;
        await membershipRepository.SaveOrganizationMembershipAsync(
            membership,
            Audit(organizationId, null, actor.UserId, subjectUserId, MembershipAuditAction.OrganizationMemberRevoked, membership.Role.ToString(), null, utcNow),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task TransferOwnershipAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid newOwnerUserId,
        CancellationToken cancellationToken = default)
    {
        var actor = await RequireOrganizationCapabilityAsync(
            actorUserId, organizationId, MembershipCapability.TransferOrganizationOwnership, cancellationToken).ConfigureAwait(false);
        if (actor.Role != OrganizationMembershipRole.Owner)
            throw new UnauthorizedAccessException("Only the active organization owner can transfer ownership.");
        var newOwner = await RequireActiveOrganizationMembershipAsync(organizationId, newOwnerUserId, cancellationToken)
            .ConfigureAwait(false);
        if (newOwner.UserId == actor.UserId)
            throw new InvalidOperationException("The selected user already owns the organization.");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await membershipRepository.TransferOwnershipAsync(
            organizationId,
            actor.UserId,
            newOwner.UserId,
            utcNow,
            Audit(organizationId, null, actor.UserId, newOwner.UserId, MembershipAuditAction.OrganizationOwnershipTransferred, newOwner.Role.ToString(), OrganizationMembershipRole.Owner.ToString(), utcNow),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task AttachVenueAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var actor = await RequireOrganizationCapabilityAsync(
            actorUserId, organizationId, MembershipCapability.ManageVenueMembers, cancellationToken).ConfigureAwait(false);
        if (entitlementService is not null)
            await entitlementService.EnsureCanAddVenueAsync(organizationId, cancellationToken).ConfigureAwait(false);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await membershipRepository.AttachVenueAsync(
            organizationId,
            RequireId(venueId, nameof(venueId)),
            Audit(organizationId, venueId, actor.UserId, actor.UserId, MembershipAuditAction.VenueAttached, null, null, utcNow),
            cancellationToken).ConfigureAwait(false);
        if (organizationSubscriptions is not null && projectionService is not null)
        {
            var subscription = await organizationSubscriptions.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("An authoritative organization subscription is required.");
            await projectionService.SyncVenueAsync(venueId, subscription, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<VenueMembership> AddOrChangeVenueMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        Guid subjectUserId,
        VenueMembershipRole role,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        await RequireVenueCapabilityAsync(actorUserId, organizationId, venueId, cancellationToken).ConfigureAwait(false);
        await RequireActiveOrganizationMembershipAsync(organizationId, subjectUserId, cancellationToken).ConfigureAwait(false);
        var existing = await membershipRepository.GetVenueMembershipAsync(organizationId, venueId, subjectUserId, cancellationToken)
            .ConfigureAwait(false);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var membership = existing ?? new VenueMembership
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, VenueId = venueId, UserId = subjectUserId, GrantedUtc = utcNow, CreatedUtc = utcNow
        };
        var previousRole = existing?.RevokedUtc is null ? existing?.Role.ToString() : null;
        membership.Role = role;
        membership.RevokedUtc = null;
        membership.UpdatedUtc = utcNow;
        var action = previousRole is null ? MembershipAuditAction.VenueMemberAdded : MembershipAuditAction.VenueMemberRoleChanged;
        return await membershipRepository.SaveVenueMembershipAsync(
            membership,
            Audit(organizationId, venueId, actorUserId, subjectUserId, action, previousRole, role.ToString(), utcNow),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task RevokeVenueMemberAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        Guid subjectUserId,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueCapabilityAsync(actorUserId, organizationId, venueId, cancellationToken).ConfigureAwait(false);
        var membership = await membershipRepository.GetVenueMembershipAsync(organizationId, venueId, subjectUserId, cancellationToken)
            .ConfigureAwait(false);
        if (membership is null || membership.RevokedUtc is not null)
            throw new InvalidOperationException("An active venue membership is required.");
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        membership.RevokedUtc = utcNow;
        membership.UpdatedUtc = utcNow;
        await membershipRepository.SaveVenueMembershipAsync(
            membership,
            Audit(organizationId, venueId, actorUserId, subjectUserId, MembershipAuditAction.VenueMemberRevoked, membership.Role.ToString(), null, utcNow),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<OrganizationMembership> RequireOrganizationCapabilityAsync(
        Guid actorUserId,
        Guid organizationId,
        MembershipCapability capability,
        CancellationToken cancellationToken)
    {
        var actor = await RequireActiveOrganizationMembershipAsync(organizationId, actorUserId, cancellationToken)
            .ConfigureAwait(false);
        if (!capabilityResolver.HasCapability(actor.Role, null, capability))
            throw new UnauthorizedAccessException("The actor does not have the required organization capability.");
        return actor;
    }

    private async Task RequireVenueCapabilityAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid venueId,
        CancellationToken cancellationToken)
    {
        RequireId(venueId, nameof(venueId));
        var organizationMembership = await membershipRepository.GetOrganizationMembershipAsync(organizationId, actorUserId, cancellationToken)
            .ConfigureAwait(false);
        var venueMembership = await membershipRepository.GetVenueMembershipAsync(organizationId, venueId, actorUserId, cancellationToken)
            .ConfigureAwait(false);
        var organizationRole = organizationMembership?.RevokedUtc is null ? organizationMembership?.Role : null;
        var venueRole = venueMembership?.RevokedUtc is null ? venueMembership?.Role : null;
        if (!capabilityResolver.HasCapability(organizationRole, venueRole, MembershipCapability.ManageVenueMembers))
            throw new UnauthorizedAccessException("The actor does not have the required venue capability.");
    }

    private async Task<OrganizationMembership> RequireActiveOrganizationMembershipAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetOrganizationMembershipAsync(
            RequireId(organizationId, nameof(organizationId)),
            RequireId(userId, nameof(userId)),
            cancellationToken).ConfigureAwait(false);
        return membership is not null && membership.RevokedUtc is null
            ? membership
            : throw new InvalidOperationException("An active organization membership is required.");
    }

    private static void ValidateAssignableOrganizationRole(OrganizationMembershipRole role)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        if (role == OrganizationMembershipRole.Owner)
            throw new InvalidOperationException("Ownership can only be assigned through an ownership transfer.");
    }

    private static MembershipAuditEntry Audit(
        Guid organizationId,
        Guid? venueId,
        Guid actorUserId,
        Guid subjectUserId,
        MembershipAuditAction action,
        string? previousRole,
        string? newRole,
        DateTime occurredUtc) => new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = organizationId,
        VenueId = venueId,
        ActorUserId = actorUserId,
        SubjectUserId = subjectUserId,
        Scope = venueId is null ? MembershipAuditScope.Organization : MembershipAuditScope.Venue,
        Action = action,
        PreviousRole = previousRole,
        NewRole = newRole,
        OccurredUtc = occurredUtc
    };

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty identifier is required.", parameterName);
}

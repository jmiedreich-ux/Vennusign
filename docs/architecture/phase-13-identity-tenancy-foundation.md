# Phase 13 Identity and Tenancy Foundation

## Decision

WP-13.01 introduces the durable customer identity and tenancy model used by the remaining Phase 13 packages. It deliberately does not change authentication handlers, issue sessions, accept passwords, or expose UI.

## Identity boundary

- `CustomerUser` is the product-owned person record. Email is trimmed and normalized deterministically for lookup and uniqueness.
- `ExternalIdentity` links a user to an approved identity provider by provider and immutable provider subject. Google and Apple are the first approved providers.
- Provider credentials, OAuth callbacks, session issuance, passwordless challenges, passkeys, TOTP, and recovery are outside this package. Authentication begins in WP-13.02.
- Password hashes and reusable secrets are not stored by this model.

## Tenant boundary

- `Organization` is the customer tenant and has exactly one active owner.
- `OrganizationMembership` grants an organization role to a customer user. Ownership is changed only by the serialized transfer operation.
- `Venue.OrganizationId` is nullable so existing venues can be migrated deliberately rather than assigned to an invented tenant. New organization-aware operations attach a venue atomically and reject reassignment from another organization.
- `VenueMembership` is constrained by composite foreign keys to both the venue's organization and an organization membership. A cross-organization venue membership cannot be persisted.

## Authorization contract

Roles are stored assignments; capabilities are server-owned conclusions. `MembershipCapabilityResolver` maps organization and venue roles to the smallest deterministic capability set. Callers authorize using capabilities rather than interpreting role names in UI or request data.

The initial roles are:

- Organization owner: organization/member/venue administration plus ownership transfer.
- Organization admin: organization/member/venue administration without ownership transfer.
- Organization member: organization read only, augmented by any venue-specific role.
- Venue manager: venue read, content management, and member management.
- Venue editor: venue read and content management.
- Venue viewer: venue read only.

Unknown roles and capabilities fail closed.

## Mutation and audit rules

Membership creation, role change, revocation, venue attachment, and ownership transfer write their audit row in the same SQL transaction as the state change. Audit rows capture tenant, optional venue, actor, subject, action, prior/new role, and server time. Venue-scoped audit rows use the same composite venue/organization foreign key as venue memberships, and a database trigger rejects audit updates and deletes.

Organization ownership transfer uses serializable isolation and verifies both the current owner and new owner's active membership before changing the two memberships and organization owner pointer.

## Compatibility and follow-on work

- Existing back-office token authentication remains unchanged until WP-13.09 performs the approved compatibility migration.
- WP-13.02 will use these identity and membership contracts for customer authentication and session authorization.
- WP-13.03 will add passkey, TOTP, and recovery artifacts without changing this tenancy boundary.
- Trial, signup, onboarding, and customer-facing screens remain assigned to WP-13.04 through WP-13.08.

## UI and function gap analysis

Not applicable. WP-13.01 has no new or changed page or screen; its complete bounded output is persistence, repository/service contracts, authorization resolution, migration safety, and non-integration tests.

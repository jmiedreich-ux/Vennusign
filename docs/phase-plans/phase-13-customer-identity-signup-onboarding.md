# Phase 13 — Customer Identity, Signup, and Onboarding

## Implementation Status

This phase is approved for sequential implementation. Start with `WP-13.01` only after it has its own claimed GitHub issue, branch, work-package record, and pull request.

## Provisional Roadmap Sequencing

- `Phase 13` and its `WP-13.xx` identifiers are approved for this implementation sequence.
- Roadmap phases after Phase 13 remain paused. Their numbering, names, and order may be revised before implementation is approved.
- No paused future-phase identifier creates an implementation commitment or dependency.

## Approved Objective

Deliver frictionless customer identity, signup, entitlement, venue setup, and first-screen onboarding so a customer can authenticate, choose a tier-defined no-card trial or paid plan, complete Checkout when required, create a venue, pair a first screen, and return through a real authenticated user session rather than a manually configured Venue Admin token.

## Accepted Product Decisions

- Use checkout-after-auth: authenticate first, select a no-card trial or paid tier, complete Stripe Checkout only for paid tiers, then complete venue and first-screen onboarding.
- Tier settings define trial duration, included venues/screens/features, and expiry behavior; onboarding must not hardcode trial policy.
- Support Google, Apple, passkeys, TOTP, and email-link recovery/fallback. Do not plan Facebook or X/Twitter providers.
- Prefer passkeys for returning users. Use enrolled TOTP for fallback and step-up authentication. Retain email links for fallback and recovery.
- Show resumable horizontal onboarding timelines in the customer/Venue Admin experience and the internal Super Admin experience.
- Keep existing config-backed Venue Admin tokens only as a temporary legacy/local compatibility path while real user authorization is introduced.

## Sequential Work Packages

1. **WP-13.01 — Identity, Organization, and Membership Foundation**
   Define and persist users, external identities, organizations, organization memberships, venue memberships, ownership, roles, audit baseline, and authorization boundaries.
2. **WP-13.02 — Customer Authentication Foundation**
   Add Google and Apple sign-in, verified identity/account-linking rules, session boundaries, and email-link fallback/recovery without a password-login flow.
3. **WP-13.03 — Passkeys, TOTP, and Account Recovery**
   Add passkey enrollment/sign-in, TOTP enrollment and recovery codes, recent-authentication/step-up rules, and secure recovery behavior.
4. **WP-13.04 — Tier-Defined Trials and Stripe Entitlements**
   Resolve no-card trial behavior from tier settings, add paid Checkout activation, keep Stripe webhooks authoritative, and enforce venue/screen/feature entitlements.
5. **WP-13.05 — Public Signup and Resumable Onboarding**
   Add public signup/sign-in routes and persisted onboarding state for identity, plan selection, entitlement, venue setup, and first-screen progress.
6. **WP-13.06 — Venue Setup and First-Screen Activation**
   Add venue details, entitlement-aware initial screen creation, physical display pairing handoff, and a clear distinction between screen creation and an active paired device.
7. **WP-13.07 — Customer Onboarding Timeline**
   Add the customer/Venue Admin horizontal timeline: Account, Plan, Venue, First Screen, and Go Live.
8. **WP-13.08 — Super Admin Onboarding Visibility**
   Add the Super Admin venue-support timeline with onboarding state, plan/trial status, first-screen progress, last activity, and safe operational support actions.
9. **WP-13.09 — Legacy Venue Access Token Migration**
   Define and implement the compatibility, migration, revoke, and retirement path for config-backed Venue Admin access tokens.
10. **WP-13.10 — Phase 13 Validation and Closure**
    Run the full non-integration regression suite and validate authentication, authorization, tier/trial entitlements, onboarding recovery, Stripe webhook authority, pairing, security, migration, and documentation consistency.

## Architecture and Security Decisions Assigned to the Foundation

- `WP-13.01` must establish the identity, organization, membership, role, capability, and audit boundaries needed by all later packages.
- `WP-13.02` and `WP-13.03` must resolve external identity keys, verified-email assurance, account linking, email collision, session boundaries, OAuth/OIDC state/nonce/PKCE, passkey/TOTP, recovery, and step-up rules.
- `WP-13.04` must resolve Stripe customer ownership, paid and no-card trial lifecycles, expiry/grace behavior, and entitlement enforcement.
- `WP-13.05` and `WP-13.06` must resolve the persisted onboarding state machine, draft/active venue rules, first-screen state, pairing state, and resumability behavior.
- `WP-13.09` must define and implement the compatibility and removal plan for existing `VenueAdmin:Sessions` configuration-backed tokens.

## UI Design and Function Gap Analysis

Before designing or implementing any new or changed UI page or screen:

1. Consult the available UX best-practices MCP first and record the applicable recommendations in the work package or PR.
2. Perform and record a page/screen gap analysis covering:
   - primary user goals and key tasks;
   - information hierarchy, page layout, and responsive/accessibility needs;
   - navigation placement, route relationships, and avoidance of redundant navigation;
   - required create, read, edit, and delete actions when the surface manages data;
   - empty, loading, error, success, permission-denied, and disconnected/offline states as applicable;
   - input validation, confirmation/undo behavior for destructive actions, and actionable feedback;
   - required API endpoints, data contracts, authorization, entitlement, realtime, and persistence support.
3. Resolve every required gap within the package or explicitly document why it is intentionally excluded or deferred to a linked package.

No UI package is complete if its screen lacks necessary user actions, essential states, or coherent non-redundant navigation.

## Phase Boundaries

- No Facebook or X/Twitter authentication providers.
- No password-login flow in the initial Phase 13 design.
- No committed provider secrets, private keys, recovery codes, or local machine credentials.
- No simulated Stripe success; paid entitlements remain webhook-authoritative.
- No replacement of existing Venue Admin authorization without an approved compatibility and migration plan.
- No external-provider, Azure SQL, hosted-infrastructure, container, or cross-system integration tests under the standing owner exception.

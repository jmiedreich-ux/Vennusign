# Phase 13 Customer Onboarding

## Decision

WP-13.05 introduces one resumable onboarding state per authenticated customer and, after organization creation, one state per organization. The server derives progress from this durable state plus the authoritative organization subscription. Browser routes and Stripe return parameters are presentation inputs only; neither can grant entitlement.

## State ownership

- `CustomerOnboardingStates.UserId` is the primary key and always comes from the authenticated customer session claim.
- `OrganizationId` is unique when present, preventing two customer journeys from claiming the same organization.
- Organization creation uses `IIdentityMembershipService`, so the current customer becomes the owner under the WP-13.01–13.04 identity model.
- Clients never supply a user or organization identifier to onboarding mutation endpoints. This prevents cross-customer state access and insecure direct-object references.
- `SelectedTierId` records intent. `OrganizationSubscriptions` remains the only entitlement authority.

## State machine

1. `account`: an authenticated customer has no organization yet.
2. `plan`: the organization exists but has no current trial or active subscription.
3. `venue`: entitlement is confirmed but no venue has been attached to onboarding.
4. `first-screen`: the venue exists but no first screen has been attached.
5. `go-live`: the first screen exists.

WP-13.05 owns the account and plan transitions. WP-13.06 owns the venue and first-screen transitions. Persisting the later identifiers now gives WP-13.06 a stable continuation contract without implementing its UI or provisioning behavior early.

## Public and authenticated API boundary

- `GET /api/customer-onboarding/plans` is anonymous and returns only active, public tier policy. Provider price IDs are reduced to availability booleans and are not exposed.
- All state and mutation endpoints require the customer-authentication policy and secure customer-session cookie.
- Organization creation is single-use for a journey.
- Trial activation uses the selected tier's configured `TrialDays`; it is rejected when the tier is not public/active, has no trial, or the organization is already entitled.
- Paid selection delegates to organization-owned Stripe Checkout. Checkout return success is reported as pending until a verified webhook changes `OrganizationSubscriptions`.

## Browser boundary

The Venue Admin frontend exposes `/signup`, `/signin`, and `/onboarding` as public customer-entry routes. Google and Apple use the established external-provider endpoints. Returning customers can use the established email-link and passkey flows. All API requests use credentialed cookies, and hosted Checkout navigation passes the existing HTTPS Stripe-origin allowlist.

The UI provides loading, error, empty-plan, sign-in, saved-progress, Checkout-canceled, webhook-pending, and entitlement-confirmed states. Native form constraints, semantic labels, status/alert regions, visible focus, ordered progress, and a responsive single-column layout support keyboard, assistive-technology, and narrow-screen use.

## Security and operational boundaries

- No provider secret, private key, recovery code, or legacy Venue Admin token is stored in onboarding state or returned to the browser.
- Cross-origin customer-session use is limited to configured CORS origins with credentials; wildcard origins are not enabled.
- No destructive onboarding action exists in WP-13.05. Sign-out revokes only the current session and leaves durable progress intact.
- Integration, Azure SQL, live identity-provider, live Stripe, hosted-infrastructure, container, and physical-device validation remains skipped under the standing owner exception. Focused unit, migration-contract, frontend source-contract, TypeScript, and production-build validation cover this package.

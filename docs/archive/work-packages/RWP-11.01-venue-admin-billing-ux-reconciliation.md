# RWP-11.01 — Venue Admin Billing UX Reconciliation

## Status

Complete.

## Goal

Relocate the customer upgrade experience delivered by WP-11.01 through WP-11.05 from the internal Super Admin application into the venue-scoped Venue Admin CMS before Stripe Checkout work begins.

## Scope

- Add a venue-claim-bound billing presentation endpoint for current tier, active public tier prices, and effective features.
- Move the canonical upgrade catalog, tier badges, locked navigation and previews, inline hints, rotating sidebar suggestion, and upgrade modal into Venue Admin.
- Coordinate the surfaces so only one suggestion is active at a time.
- Preserve session-scoped dismissal, reduced-motion behavior, and the established ten-month annual price presentation.
- Remove customer upgrade orchestration from Super Admin while retaining internal tier switching, entitlement visibility, and feature overrides.

## Acceptance Criteria

1. Billing presentation derives the venue only from the authenticated Venue Admin claim.
2. The response excludes Stripe identifiers, secrets, and raw provider data.
3. Venue Admin owns the WP-11.01–11.05 customer experience and shows no more than one active suggestion.
4. Existing customer workflows remain operable, visible, and tier-aware.
5. Super Admin retains support tools but mounts no customer upgrade prompt or modal.
6. The upgrade CTA performs no Stripe request or entitlement mutation.
7. Required non-integration checks pass; integration-type tests remain skipped.

## Boundaries

- No Checkout session, redirect, Billing Portal, Stripe network call, webhook change, or entitlement mutation.
- No change to authoritative feature resolution or subscription lifecycle behavior.
- No Phase 12 work.

## GitHub

- Issue: #264
- Branch: `rwp/11.01-venue-admin-billing-ux-reconciliation`
- Pull request: #265

## Validation Evidence

- Local Venue Admin tests: 17 passed; production build passed.
- Local Super Admin tests: 73 passed; production build passed.
- Local .NET validation: not run because the SDK is unavailable in this workspace.
- GitHub Actions run #565: passed against reviewed head `f3b002516f6d25c5efe300451e6eb917679602ca`.
- Merge commit: `484f2fb4be7695089961a2da3f68981b1d6be57f`.
- Review: `CHATGPT APPROVED` recorded against the exact reviewed head.
- Skipped: Azure SQL, external Stripe/service, credentialed, hosted-infrastructure, container, and all other integration-type tests under the standing owner exception.

## Next

Resume the approved Phase 11 sequence with WP-11.06 — Stripe Checkout Session Foundation.

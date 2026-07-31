# Vennu Session Handoff

## Work Package

- ID: WP-11.07
- Status: Available
- Execution mode: Sequential

## Git State

- Branch: pending
- Issue: pending
- Pull request: pending
- Latest reviewed commit: `2a5e7a4e269700fc5bdbe428339d7b8f77c0fb60` (WP-11.06)
- Merge commit: `f3380b060f1732d725ea25d0b56b9164db169b7c` (WP-11.06)
- CI state: WP-11.06 Actions run #572 passed

## Completed This Session

- Completed and merged RWP-04.01 through PR #260.
- Added protected Super Admin venue provisioning.
- Added normalized venue validation, deterministic Starter-tier trial initialization, and focused tests.
- Added the independent `src/venue-admin` SPA and protected venue-scoped bootstrap.
- Added capability-aware shell navigation with deterministic locked states.
- Added independent Venue Admin build/test coverage to GitHub Actions.
- Moved menu section, item editing, presentation, and Quick Update UI into Venue Admin.
- Added a venue-claim-bound menu API that accepts no browser-supplied venue ID.
- Replaced the Super Admin menu editor with a bounded Venue Admin handoff.
- Moved screens, themes, scheduling, promotions, broadcasts, playlists, and tap management into Venue Admin.
- Added venue-claim enforcement across every mirrored operational controller and pairing flow.
- Refocused Super Admin venue detail on support, tier, entitlement, and override context.
- Added a claim-bound Venue Admin billing presentation contract without Stripe identifiers.
- Moved the WP-11.01–11.05 upgrade catalog, prompts, locked previews, and modal into Venue Admin.
- Removed customer upgrade orchestration from Super Admin while retaining support tier and override controls.
- Completed the approved RWP queue and restored normal Phase 11 roadmap order.
- Added the claim-bound Stripe Checkout Session foundation with public catalog validation and an allowlisted hosted-URL response.

## Decisions

- Venue creation reuses the established venue repository and trial service; no new database table or subscription path was introduced.
- The initial commercial state is the active seeded `starter` tier with the existing 14-day trial behavior.
- Super Admin remains the internal provisioning surface; venue-facing daily management moves in RWP-05.01 through RWP-05.03.

## Validation

- Results: Actions run #553 passed the required build and non-integration validation.
- Local results: 73 Super Admin tests and the Super Admin production build passed.
- RWP-05.01: Actions run #556 passed all required non-integration validation.
- RWP-05.02 local validation: 7 Venue Admin tests and 73 Super Admin tests passed; both production builds passed.
- RWP-05.03 local validation: 12 Venue Admin tests and 73 Super Admin tests passed; both production builds passed.
- RWP-05.03: Actions run #562 passed all required non-integration validation.
- RWP-11.01 local validation: 17 Venue Admin tests and 73 Super Admin tests passed; both production builds passed.
- RWP-11.01: Actions run #565 passed all required non-integration validation.
- WP-11.06: Actions run #572 passed all required non-integration validation.
- Skipped: local .NET validation because the SDK is unavailable.
- Standing skip: all Azure SQL, external-service, credentialed, hosted-infrastructure, container, and other integration-type tests.

## Remaining Work

- WP-11.07 — Checkout Launch and Entitlement Return.

## Exact Next Action

Claim WP-11.07 and connect the Venue Admin upgrade CTA to Checkout with bounded success/cancel return states and authoritative post-webhook refresh.

## Do Not Redo or Reverse

- Do not grant entitlements from Checkout return parameters; webhook-processed subscription state remains authoritative.
- Do not move venue-operator workflows into Super Admin; RWP-05.01 establishes the separate Venue Admin CMS.
- Do not implement Billing Portal or contract billing before WP-11.08 and WP-11.09.

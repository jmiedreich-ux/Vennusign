# Vennu Session Handoff

## Work Package

- ID: WP-11.06
- Status: Available
- Execution mode: Sequential

## Git State

- Branch: pending
- Issue: pending
- Pull request: pending
- Latest reviewed commit: `f3b002516f6d25c5efe300451e6eb917679602ca` (RWP-11.01)
- Merge commit: `484f2fb4be7695089961a2da3f68981b1d6be57f` (RWP-11.01)
- CI state: RWP-11.01 Actions run #565 passed

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
- Skipped: local .NET validation because the SDK is unavailable.
- Standing skip: all Azure SQL, external-service, credentialed, hosted-infrastructure, container, and other integration-type tests.

## Remaining Work

- WP-11.06 — Stripe Checkout Session Foundation.

## Exact Next Action

Claim WP-11.06 and implement its authenticated venue-scoped Checkout session foundation.

## Do Not Redo or Reverse

- Do not add public self-service signup or Stripe Checkout to this remediation package.
- Do not move venue-operator workflows into Super Admin; RWP-05.01 establishes the separate Venue Admin CMS.
- Do not connect Checkout or mutate entitlement until WP-11.06 and WP-11.07.

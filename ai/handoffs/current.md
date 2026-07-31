# Vennu Session Handoff

## Work Package

- ID: RWP-05.03
- Status: Review
- Execution mode: Sequential

## Git State

- Branch: `rwp/05.03-venue-operations-migration`
- Issue: #256
- Pull request: pending
- Latest reviewed commit: `07d585adffd35d7291bd22b51a687a24b280eee6` (RWP-05.02)
- Merge commit: `5bc870fceb145c14f42ce9dc9a8c2f074e6b28a4` (RWP-05.02)
- CI state: RWP-05.02 Actions run #559 passed

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
- Skipped: local .NET validation because the SDK is unavailable.
- Standing skip: all Azure SQL, external-service, credentialed, hosted-infrastructure, container, and other integration-type tests.

## Remaining Work

- Validate, review, and merge RWP-05.03.

## Exact Next Action

Publish RWP-05.03 and use GitHub Actions as the authoritative non-integration gate.

## Do Not Redo or Reverse

- Do not add public self-service signup or Stripe Checkout to this remediation package.
- Do not move venue-operator workflows into Super Admin; RWP-05.01 establishes the separate Venue Admin CMS.

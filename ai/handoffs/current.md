# Vennusign Session Handoff

## Current State

- Item: RWP-05.06 — Back Office Organization and Venue Context / issue #419
- Mode: Sequential
- Branch: `rwp/05.06-back-office-context`
- Status: Complete in the proposed merge state

## Result

- Every authenticated Back Office route shows the active organization and venue separately from the signed-in account.
- Multi-venue accounts receive a native, responsive selector containing only server-authorized manageable contexts; single-context and legacy sessions remain explicit and non-switchable.
- Switching requires confirmation, retains the old context on failure, announces outcomes, remounts tenant data screens, and refreshes billing.
- Browser persistence occurs only after successful server validation. Stale saved context is cleared and recovered through the server-selected onboarding venue.
- The authorization boundary and UI/function gap analysis are recorded in `docs/architecture/back-office-context.md` and `docs/archive/work-packages/RWP-05.06-back-office-organization-venue-context.md`.

## Validation

- Focused Back Office Node tests pass locally (51/51).
- Back Office production build passes locally.
- Focused .NET checks are delegated to exact-head affected-area GitHub Actions because local .NET tooling is unavailable.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

After this RWP merges and its claim is released, reassess and claim only RWP-08.01 / issue #346 in Sequential mode if it has no active owner.

## Do Not Redo

Do not trust a browser venue ID as authorization, expose contexts without manage-content permission, merge account and tenant identity labels, carry old tenant state through a switch, weaken venue-scoped controller checks, skip the recorded queue, or resume Phase 14+.

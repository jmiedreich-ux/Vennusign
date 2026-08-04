# RWP-05.08 — Screens Page Information Architecture

## Outcome

Back Office Screens separates daily operation, hardware setup, and capacity/video-wall planning into three explicit workflow regions. Setup starts open when a venue has no active screen and collapses after the first screen is created or paired. Layout controls now remain local drafts until the operator explicitly applies or discards them.

## Required implementation

- Group delivery targeting, fleet health, filtering, and screen controls under **Daily**.
- Group add, pair, and replacement workflows under a collapsible **Setup** region.
- Group deterministic overflow and video-wall workflows under **Capacity & walls**.
- Start Setup open for a venue without active screens and collapsed for an established fleet; preserve manual disclosure control.
- Replace immediate layout/density/ratio/dwell writes with per-screen drafts and explicit **Apply to TV** / **Discard changes** actions.
- Preserve every existing screen, delivery, preview, replacement, capacity, and video-wall operation.

## UI and function gap analysis

- **Goal and hierarchy:** daily service actions appear in the primary open region. Less frequent hardware setup and planning work have named boundaries, eliminating the previous eight-workflow column without hiding any capability.
- **Navigation:** semantic headings establish Daily, Setup, and Capacity & walls landmarks. Setup uses a native keyboard-operable disclosure; it is open for first-screen onboarding and collapses after successful create or pair.
- **Required actions:** Add, Pair, Replace, Preview, Push, identity Save/Cancel, layout Apply/Discard, Select target, Reset, Archive, Restore, Unpair, overflow selection, and video-wall actions remain reachable.
- **Loading, empty, error, success, and permission states:** existing loading, filter-empty, load failure, mutation error, delivery state, plan quota, and success notices remain in context. The no-screen message points to Setup. Disabled Pro layouts and the video-wall capability boundary remain visible and honest.
- **Validation:** pairing codes retain numeric six-digit validation; names and locations retain limits; layout values remain the existing bounded options. A draft never calls the API until Apply, and Discard restores the authoritative saved values.
- **Destructive actions:** the standardized RWP-00.08 review dialogs remain unchanged for replacement, reset, archive, and unpair. Grouping does not weaken exact-name typed confirmation for unpairing.
- **Feedback:** the draft bar states that the TV is unchanged, names the pending state, and provides Apply/Discard beside it. Existing API success/error and delivery acknowledgement feedback remains authoritative.
- **Accessibility:** native `details`/`summary`, semantic headings, explicit labels, live regions, disabled states, visible focus, and non-color status labels remain. The disclosure indicator is decorative and the text carries the meaning.
- **Responsiveness:** all regions reduce to one column at existing breakpoints; the draft actions stack on phones; Setup and its controls remain usable without horizontal scrolling.
- **API, data, authorization, and entitlement support:** no endpoint, payload, migration, authorization, tenant, or entitlement changes are required. Existing venue-scoped screen APIs and capability checks remain authoritative.

## Accepted exclusions

- Live-thumbnail cards, per-card overflow action menus, and delivery ribbons belong to RWP-05.10.
- The daypart Home and grouped application navigation belong to RWP-05.09.
- Global action hierarchy belongs to RWP-00.13; global toasts belong to RWP-00.09.
- This package does not alter delivery receipts, player behavior, or video-wall contracts.

## Validation

- Back Office production build: passed locally.
- Back Office Node tests: 75 passed locally.
- Focused tests cover section boundaries, first-screen Setup behavior, draft persistence, explicit Apply/Discard, and the absence of instant layout saves.
- Exact-head affected-area GitHub Actions is authoritative before merge.

## Skipped integration testing

Browser automation, live TV mutation verification, Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped under the standing owner instruction.

## Queue and boundaries

- Issue: #452.
- Branch: `rwp/05.08-screens-information-architecture`.
- RWP-04.03 / #453 becomes next only after this PR merges, issue #452 closes, `master` is verified, and the claim is released.
- RWP-13.06 / #466 remains held; Phase 14+ remains paused.

# Proposed design references

These files preserve visual concepts that are under owner review.

## Vennusign onboarding flow to live menu

- File: `vennusign-onboarding-flow-to-live-menu.png`
- Status: **Proposed — not yet approved**
- Intended use: visual baseline for the next onboarding revision if the owner explicitly approves it.
- Approval rule: repository presence does not constitute design approval. Implementation RWPs may reference this image as approved only after explicit owner confirmation.

## Platform Operations screens

- Files: `platform-operations/po-screens.html` (wireframes) and `platform-operations/screenshots/` (10 exported PNGs: release board, release detail, rollout progress, cohort health, organizations, organization profile, customer versions, register a venue, version inventory, window schedule).
- Status: **Proposed — not yet approved**
- Related concept: `../progressive-customer-cutover-concept.md` describes the Version Router / progressive rollout concept these screens visualize.
- Approval rule: repository presence does not constitute design approval. Implementation RWPs may reference these screens as approved only after explicit owner confirmation.

## Stale-screen signals on the guest board

- Files: `display-stale-signals.md` (proposal), `display-stale-signals.html` (interactive wireframes), `display-stale-signals.png` (full sheet) and `display-stale-signals-00.png` … `-06.png` (per-treatment crops; `-00` is the current build).
- Status: **Proposed — not yet approved**
- Intended use: six treatments for signalling that a board has stopped receiving updates, replacing the guest-visible "Live updates unavailable" banner in `src/display/src/displayPresentation.mjs`. Built on the decided precedent that guest copy and staff copy differ for the same fact (owner amendment 2026-08-13, "Guest copy is Sold out; staff copy is 86") while decision 5 keeps disconnection named honestly on staff surfaces.
- Open for review: treatment 05 (withholding volatile claims when stale) changes what the board asserts and needs its own owner decision; the 15- and 60-minute thresholds are placeholders pending real 86 frequency.
- Approval rule: repository presence does not constitute design approval. Implementation RWPs may reference these treatments as approved only after explicit owner confirmation.

## Observability, correlation and performance telemetry

- File: `observability-and-performance-telemetry.md`
- Status: **Proposed — not yet approved**
- Intended use: the evidence layer the customer support diagnostic agent concept depends on, plus the performance dimension that concept does not cover — user-perceived spans decomposed into server, network and client time, so "code it better" can be told apart from "buy more resources".
- Key positions: measure the action rather than the request (a reorder measured 3,981 ms while the API call behind it returned 204 in under a second); percentiles rather than averages; OpenTelemetry as wire format with W3C trace context everywhere, full SDK only where it is cheap; identifiers and outcomes in logs, never customer content.
- Not blocked on: acting-as-venue-admin. `docs/architecture/administrative-identity.md` already settles Platform Operations as a separate application identity, so tiers 1 and 2 need no new permission surface.
- Approval rule: repository presence does not constitute design approval. Implementation RWPs may reference this as approved only after explicit owner confirmation.

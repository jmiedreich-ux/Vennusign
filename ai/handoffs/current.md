# Vennusign Session Handoff

## Current State

- Item: RWP-05.08 — Screens Page Information Architecture / issue #452
- Mode: Sequential
- Branch: `rwp/05.08-screens-information-architecture`
- Status: Implemented and locally validated; pending exact-head Actions, review, and merge

## Approved Queue

RWP-04.03 / issue #453 is next after RWP-05.08 fully merges, closes, verifies, and releases its claim. The remaining approved queue continues through RWP-11.04 / issue #465. RWP-13.06 / issue #466 is held and excluded. Phase 14+ remains paused.

Each package must be claimed, implemented, validated, merged, and released before the next package is claimed. This run has reached its five-package limit after RWP-05.08 completes.

## RWP-05.08 Proposed Outcome

- Screens is divided into Daily, Setup, and Capacity & walls workflow regions.
- Setup is open for a venue with no active screens and collapses after a successful create or pair.
- Layout, density, ratio, and hero dwell changes remain per-screen drafts until Apply to TV.
- Discard restores the authoritative saved presentation without an API call.
- Existing screen lifecycle, delivery, preview, capacity, video-wall, authorization, and entitlement behavior remains unchanged.

## Boundaries

- Do not broaden RWP-05.08 into RWP-05.10 live-thumbnail cards, RWP-00.13 overflow action hierarchy, or RWP-00.09 toast behavior.
- Do not claim RWP-04.03 or any later item before the current claim is fully released.
- Do not claim or implement held RWP-13.06 / issue #466.
- Do not resume Phase 14+.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

Publish the RWP-05.08 implementation PR, require affected Back Office GitHub Actions on the exact reviewed head, review and merge it, close issue #452, verify `master`, and release the claim. RWP-04.03 / issue #453 is the next approved item for the next run only after that sequence completes.

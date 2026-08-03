# Vennusign Session Handoff

## Current State

- Item: RWP-05.07 — Atomic Screen Replacement and Pairing Recovery / issue #439
- Mode: Sequential
- Branch: `rwp/05.07-atomic-screen-replacement`
- Status: Implemented; pending exact-head CI, review, and merge

## Approved Queue

1. RWP-08.02 / #440 — daylight-saving-safe scheduling resolution
2. RWP-10.02 / #441 — durable player content receipts and delivery reconciliation
3. RWP-00.05 / #442 — affected-screen action completeness and recovery

Each package must be claimed, implemented, validated, merged, and released before the next package is claimed. The scheduled run may complete up to five packages.

## RWP-05.07 Proposed Outcome

- Replacement retains the selected logical screen ID, configuration, history relationships, targeting, and video-wall placement.
- Pairing claim, credential rotation, logical assignment, temporary registration retirement, and audit evidence commit atomically.
- Repeated successful requests are idempotent; stale previews and conflicting or invalid codes fail safely.
- Back Office requires an impact preview and explicit confirmation, with cancel and recoverable error states.

## Boundaries

- Do not broaden RWP-00.04 into the full deployment control plane.
- Do not merge the scopes of the later remediation packages into RWP-00.04.
- Do not resume Phase 14+.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped.

## Exact Next Action

Review RWP-05.07 at its exact PR head, require affected-area GitHub Actions, merge and close issue #439, then claim RWP-08.02. Continue in recorded queue order only after the prior claim is released.

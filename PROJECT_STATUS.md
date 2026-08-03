# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Active product WP/RWP: none claimed.
- The 13-package retrospective remediation round is complete.
- RWP-00.04 — Deployment Component Versioning and Release Manifest (#437) is complete.
- RWP-05.07 — Atomic Screen Replacement and Pairing Recovery (#439) is implemented, pending exact-head CI and merge.
- Three follow-up RWPs remain approved in this exact Sequential order:
  1. RWP-08.02 — Daylight-Saving-Safe Scheduling Resolution (#440)
  2. RWP-10.02 — Durable Player Content Receipts and Delivery Reconciliation (#441)
  3. RWP-00.05 — Affected-Screen Action Completeness and Recovery (#442)
- Next available work package after RWP-05.07 merges: RWP-08.02.
- Research program `INT-TESTING-001` remains documented but is not part of this remediation queue.

## Completed Delivery

| Phase | Result |
| --- | --- |
| 02 | Core backend, display boot, realtime updates, and heartbeat lifecycle |
| 03 | Tier, feature, subscription, Stripe, and usage foundations |
| 04 | Protected Platform Operations CRM, support, commercial, and revenue workflows |
| 05 | Back Office CMS, menus, quick update, screens, and video walls |
| 06 | Restaurant/cafe layouts, themes, overflow, and offline media |
| 07 | Bar layouts, advanced themes, motion, and multilingual font delivery |
| 08 | Scheduling, playlists, promotions, and emergency broadcast |
| 09 | Tap administration, brewery layouts, and pairing registration |
| 10 | Android/Fire TV, Tizen, webOS, HaaS provisioning, and fleet health |
| 11 | Upgrade UX, Checkout, Billing Portal, and HaaS billing guardrails |
| 12 | Square, Toast, and Clover integrations through a shared POS model |
| 13 | Customer identity, organization entitlements, signup, onboarding, and legacy-token migration |

Cross-cutting remediation completed after Phase 13 includes RWP-00.02, RWP-00.03, RWP-04.02, RWP-05.04, RWP-05.05, RWP-05.06, RWP-08.01, RWP-09.01, RWP-10.01, RWP-11.02, RWP-13.03, RWP-13.01, and RWP-13.02. A retrospective requirements/code/test audit identified the bounded follow-up packages now recorded in the approved queue.

Completed WP/RWP details, phase plans, validation evidence, and earlier status snapshots are retained under `docs/archive/` for deliberate research.

## Validation Policy

Normal work uses affected-area non-integration validation. Full non-integration validation is reserved for phase closure and the exceptions defined in `AGENTS.md`. Integration and external-system tests remain skipped under the standing owner instruction unless separately approved.

## Next Action

Complete exact-head review, CI, and merge for RWP-05.07 / issue #439, then claim RWP-08.02. Continue in exact queue order, up to the five-package run limit. Phase 14 and later remain paused.

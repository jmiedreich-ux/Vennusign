# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Active product WP/RWP: none claimed in the proposed merge state.
- RWP-00.03 — Administrative Surface and Technical Identity Migration: complete.
- RWP-04.02 — Platform Operations Safety and Support Workflows: complete.
- RWP-05.04 — Back Office Navigation and Menu Lifecycle: complete.
- RWP-05.05 — Screen, Theme, and Pairing Lifecycle Recovery: complete in the proposed merge state.
- RWP-05.06 — Back Office Organization and Venue Context: complete in the proposed merge state.
- RWP-08.01 — Scheduling and Live-Control Safety: complete in the proposed merge state.
- RWP-09.01 — Tap-List Lifecycle and Operational Scale: complete in the proposed merge state.
- RWP-10.01 — Player Runtime, Targeting, and Realtime Delivery Reliability: complete in the proposed merge state.
- RWP-11.02 — Billing Tier and Downgrade Safety: complete in the proposed merge state.
- Approved Sequential remediation queue: RWP-13.03, RWP-13.01, then RWP-13.02.
- Next available package after merge and claim release: RWP-13.03 / issue #421.
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

Cross-cutting remediation completed after Phase 13 includes RWP-00.02, which standardizes the visible product name as Vennusign; RWP-00.03, which establishes Back Office and Platform Operations as the canonical administrative application identities across routes, authentication, namespaces, configuration, local tooling, CI, and persisted configuration metadata; RWP-04.02, which adds recoverable support drilldowns and deliberate impact confirmation to Platform Operations; RWP-05.04, which makes Back Office menu navigation, menu selection, lifecycle actions, ordering, Quick Update recovery, and authorized POS entry points operational; RWP-05.05, which adds safe screen lifecycle recovery, pairing guidance, active fleet capacity/filtering, video-wall edit/removal safety, and venue-wide theme scope/reset/readability feedback; RWP-05.06, which separates customer identity from persistent server-authorized organization/venue context throughout Back Office; RWP-08.01, which makes scheduling and live overrides navigable, target-explicit, ordered, recoverable, and confirmation-safe; RWP-09.01, which adds dependency-safe tap lifecycle controls, descriptions, large-list filtering, placement visibility, bounded bulk availability, and retryable live-update feedback; RWP-10.01, which restores automatic player/status recovery, kiosk-safe rendering, target-explicit structured pushes, and honest queued-delivery feedback; and RWP-11.02, which adds server-enforced billing-tier eligibility, usage/limit comparison, feature-loss review, and webhook-authoritative provider recovery while keeping HaaS separate. Bounded legacy aliases remain observable and fail closed during migration.

Completed WP/RWP details, phase plans, validation evidence, and earlier status snapshots are retained under `docs/archive/` for deliberate research.

## Validation Policy

Normal work uses affected-area non-integration validation. Full non-integration validation is reserved for phase closure and the exceptions defined in `AGENTS.md`. Integration and external-system tests remain skipped under the standing owner instruction unless separately approved.

## Next Action

After RWP-11.02 merges and releases, claim RWP-13.03 / issue #421 in Sequential mode only if it has no active owner. Continue in the recorded queue without skipping. Phase 14 and later remain paused and must not be planned, claimed, or implemented.

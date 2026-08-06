# Vennusign Project Status

## Current State

- Phase 13: complete.
- Phase 14 and later: paused.
- Product implementation: the first scheduled Track 1 chunk is complete, merged and exact-head validated; implementation is stopped for owner acceptance.
- RWP-13.06 — Trial-First Onboarding: held and must not resume unchanged.
- Native-industry Track 0 gate: complete.
- Track 0 consolidation: complete through RWP-00.81.
- Final Track 0 industry planning validation and handoff: complete in this proposed merge.
- Track 1.01 — Canonical Capability Model and Current-Code Reconciliation (#640): complete, merged and verified on `master` through PR #645.
- Track 1.02 — Server Capability Decision and Reason Contract (#641): complete, merged and verified on `master` through PR #646.
- Track 1.03 — Scoped Permission and Authority Model (#642): complete, merged and verified on `master` through PR #647.
- Track 1.04 — Essential Core and Current Gate Replacement (#643): complete, merged and verified on `master` through PR #648.
- Track 1.05 — Track Validation and Handoff (#644): complete, merged and verified on `master` through PR #650 and Actions run 31049451685.
- Track 1 closure: owner acceptance pending after Track 1.05 merges.
- Track 1 acceptance QA: all 19 owner-acceptance cases pass. Fourteen are asserted deterministically by the Playwright suite in `tests/ui`; five subjective cases (4-1, 5-0, 6-1, 6-2, 6-3) are judged by hosted agents through `scripts/run-track1-qa.ps1`. Latest gate record: `artifacts/track1-qa/20260806T030415Z/track-1-owner-acceptance.qa.json` (0 attention, 0 manual review, 0 lane failures).
- Defects found and fixed during Track 1 QA: menu item updates failing with an unbound `@Id` (missing RepoDb table mappings); the POS webhook worker terminating the whole API host; a `SERIALIZABLE` isolation level leaking onto pooled connections; an unscoped `aside` rule covering page content; capability refusals rendering as generic load failures; a menu save race persisting pre-edit values; screen thumbnails heartbeating their own screens Online; a destructive dialog that did not trap focus; and mobile navigation with no collapse control.

## Track 0 Deliverables

- industry definitions, subtypes, terminology, operating characteristics, required and optional capabilities;
- capability classification and cross-industry normalization;
- current-product feature/gate/limit inventory and reconciliation;
- unified Free/Operate/Coordinate/Portfolio/Enterprise direction;
- independent add-on architecture;
- typed limits, scope, inheritance, downgrade, exception, and active-output protection policy;
- tier and screen-capacity separation;
- Free plus paid-trial model;
- sold-tier version immutability, retirement, billing continuity, promotions, upgrades, downgrades, and migration campaigns;
- onboarding first-value journey;
- shared tier/industry dashboard direction and compact wireframe baseline;
- native and source-dependent KPI/analytics rules;
- cross-industry journey validation and implementation handoff.

Primary records:

- `track0/consolidation/OWNER_APPROVAL_AND_IMPLEMENTATION_HANDOFF.md`
- `track0/consolidation/RWP-00.79_OWNER_TIER_LIFECYCLE_DECISIONS.md`
- `track0/consolidation/FINAL_INDUSTRY_PLANNING_VALIDATION_HANDOFF.md`

## Final Result

Track 0 industry planning is complete. Industry affects terminology, defaults, starter content, recommendations, dashboard emphasis, and suggested add-ons, but does not grant entitlement. Essential manual operation remains universal. Tier outcomes, active screen capacity, permissions, product state, add-ons, typed limits, privacy/rights/safety, exceptions, and rollout remain separate.

The approved first-value journey is signup, industry/subtype selection, organization and venue setup, starter content, one-screen pairing, explicit preview/publish, heartbeat plus expected-content acknowledgement, and then a recommended Free, trial, or paid path.

One shared dashboard structure is approved with Free, Operate, Coordinate, Portfolio, Enterprise, and industry overlays. KPIs require a trustworthy source, explicit definition, scope, freshness, and reconciliation.

Pricing, final names, numeric allowances, exact trial duration, taxes, contracts, provider commitments, retention durations, and final implementation order remain intentionally undecided.

## Track 1 Result

Track 1 now provides canonical Version 1 action capabilities, structured server decisions and localized reasons, scoped roles and assignments, support grant/audit foundations, typed rollout/entitlement/add-on/allowance/layout persistence, fresh mutation authorization and a Back Office navigation/action projection driven by server decisions.

Track 1.05 corrected two combined-foundation gaps: screen capacity was still projected from browser billing data, and session responses did not expose decision parameters/conditions needed for truthful allowance explanations. Both are corrected and covered by focused UI contracts. The executable owner package is `docs/acceptance/track-1-owner-acceptance.md`.

## Exact Next Action

Conduct `docs/acceptance/track-1-owner-acceptance.md` and record Pass / Fail / Needs Adjustment. Track 1 closes only after explicit owner approval; otherwise prepare additional Track 1 RWPs for a later scheduled chunk. Do not resume RWP-13.06 or implement a future track.

## Validation Policy

Exact-head GitHub Actions is authoritative. Azure SQL, live Stripe/providers, physical devices, hosted infrastructure and integration/external-system tests remain skipped. Tracks may extend beyond five RWPs; execution is organized into scheduled sequential chunks of up to five.

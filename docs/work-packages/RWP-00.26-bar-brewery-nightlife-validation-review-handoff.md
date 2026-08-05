# RWP-00.26 — Bar, Brewery & Nightlife Validation, Review & Handoff

## Status

Complete in this proposed merge state.

## Issue

#501

## Scope completed

- Reviewed RWP-00.15 through RWP-00.25 as one coherent industry profile.
- Validated Restaurant inheritance and identified no duplicate commercial baseline.
- Validated terminology, operating model, required/optional capability inventory, classification, tier proposal, onboarding, dashboard, and analytics.
- Reconciled the authoritative subtype set and documented later aliases that must not become new primary subtypes or entitlements.
- Confirmed every inventoried concern has one primary Track 0 classification.
- Confirmed essential manual operation is not tier-gated.
- Confirmed permissions, product/domain state, tier candidates, independent add-ons, limits, privacy, connections, and rollout flags remain separate.
- Clarified that full pricing/tier/add-on presentation follows first-screen activation and does not replace first value when pairing is deferred.
- Validated representative setup, daily operation, publishing/recovery, event, private-function, disconnect/manual-fallback, upgrade/downgrade, and mixed-industry journeys.
- Recorded all unresolved owner decisions and a recommendation for later cross-industry consolidation.
- Prepared queued semantic updates for Track 0 status, capability matrix, current handoff, and Bar claim release.

## Validation outcome

The Bar, Brewery & Nightlife profile is complete, internally coherent under the canonical resolutions in the final validation record, and ready for owner approval as a planning package. No product implementation is authorized.

## Final canonical corrections

- Primary subtypes remain Pub, Sports Bar, Cocktail Bar, Wine Bar, Brewery, Brewpub, Taproom, Nightclub, Lounge, plus Unspecified / General Bar.
- Music/live-entertainment remains a descriptive trait and event emphasis, not another primary subtype.
- Brewery and Brewpub remain distinct primary subtypes.
- Full pricing and tier comparison follows a first screen being up and showing useful content; pairing deferral does not trigger pricing as a substitute for first value.

## Validation and review

Reviewed against issue #501, all merged Bar industry/work-package records, the Restaurant baseline, Track 0 classification policy, queued shared-file protocol, and project-local Impeccable guidance. The final PR requires exact-head documentation validation and ChatGPT review. Integration and external-system tests remain skipped.

## Handoff

After merge, default-branch verification, issue closure, final shared-record synchronization, and claim release, the Bar, Brewery & Nightlife queue is complete through RWP-00.26.

The next approved Track 0 work is the Café, Bakery & Dessert queue at its first unfinished approved RWP according to current GitHub state. Do not start consolidation until every native-industry validation RWP is complete and the consolidation gate is satisfied. RWP-13.06 and Phase 14+ remain paused.
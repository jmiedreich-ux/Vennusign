# RWP-00.59 — Hospitality Onboarding Experience

## Status

Complete in this proposed merge state.

## Issue

- #534

## Objective

Define the complete Hospitality onboarding journey from industry/subtype recognition through property setup and one confirmed active screen. Include starter objects and content, amenities/outlets, meetings/events, wayfinding, notices, languages, deferred questions, contextual pricing/add-on introduction, accessibility, resume, and recovery. Documentation only.

## Dependency verified

- RWP-00.58 is merged, verified, closed, and released.
- The approved Hospitality classification and proposed tier architecture are authoritative planning inputs.
- RWP-13.06 remains paused.
- RWP-00.60 — Hospitality Default Dashboard (#535) is next.

## Delivered

- Added `track0/industries/hospitality-onboarding-experience.md`.
- Defined a progressive first-success path from property/subtype through one delivered screen.
- Defined subtype-aware starter objects, amenities/services/outlets, meetings/events, wayfinding, notices, languages, screen purpose, pairing/selection, preview, publish, confirmation, and first-success landing.
- Defined the Hospitality starter menu as a task/template chooser rather than a tier.
- Classified questions as required now, recommended before publish, safe to defer, or dependency-blocked.
- Kept manual operation available without integrations.
- Recorded the accepted direction that pricing and upgrades should not interrupt the path to one active screen.
- Defined contextual post-success tier and add-on introduction without a disabled-feature grid.
- Defined automatic save, resume, reconciliation, pairing interruption, partial-delivery, stale-source, and recovery states.
- Defined accessibility and responsive requirements.
- Applied project-local Impeccable `shape`, `clarify`, `harden`, and bounded `polish` guidance.

## First-success result

Onboarding succeeds only when:

1. property and subtype are identified;
2. minimal public property context exists;
3. starter content has been reviewed;
4. one screen is selected or paired;
5. public wording, language, time, and exact target are previewed;
6. publication is requested;
7. authoritative delivery is confirmed;
8. the customer reaches the task-first starter menu.

A saved draft, pairing request, publish request, or unconfirmed player state is not first-screen success.

## Pricing and packaging result

Before first-screen success, there is no mandatory tier-selection wall, disabled-feature grid, or upgrade prompt that blocks core setup. Show only unavoidable account constraints that affect the current step.

After success, introduce Operate, Coordinate, Portfolio, Enterprise, and independent add-ons contextually by customer outcome. Show what remains included, what outcome is unlocked, setup/data requirements, manual fallback, and “not now.” Industry and subtype remain non-commercial.

Final pricing, trials, limits, and commercial implementation remain unapproved.

## Deferred setup result

Detailed hierarchy, complete amenities/outlets, all events, advanced routes, additional languages, property groups, brands, approvals, campaigns, analytics, integrations, enterprise identity, managed hardware, and final tier selection may be deferred.

Deferred items remain visible with why they matter, affected scope, current fallback, and the next action. Do not show a false complete state.

## Impeccable result

The experience is outcome-led and progressive. It supports keyboard, assistive technology, non-color status, 200% zoom, long names, localization expansion, right-to-left layouts, local dates/times, reduced motion, phone through large desktop, interruption-safe forms, and actionable errors. Preserve the approved Sky Blue administrative direction.

No UI or product implementation was introduced.

## Validation

Documentation-only review confirmed:

- every issue-listed onboarding area is addressed;
- the first active screen is the primary outcome;
- pricing/add-ons do not block core setup;
- private guest data is not required;
- integrations remain optional;
- deferred questions are explicit and recoverable;
- publication success requires delivery confirmation;
- player acceptance concerns are recorded for future implementation without implementing them;
- RWP-13.06 remains paused;
- RWP-00.60 is next.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

All integration and external-system testing and all product implementation, including onboarding UI, player behavior, pairing, API, schema, migrations, billing, entitlements, permissions, privacy systems, localization, analytics, external connections, identity, AI, hardware, pricing, trials, limits, and RWP-13.06.

## Exact next action

After this RWP is merged, verified on `master`, issue #534 is closed, and the claim is released, execute **RWP-00.60 — Hospitality Default Dashboard** (#535).

RWP-00.60 must define the task-first dashboard and starter menu shown after onboarding, remain documentation-only, and hand off to RWP-00.61.
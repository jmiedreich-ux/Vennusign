# Vennusign Project Status

## Current State

- Phase 13: complete.
- Phase 14 and later: paused.
- Product implementation: paused.
- RWP-13.06 — Trial-First Onboarding: held and must not resume unchanged.
- Native-industry Track 0 gate: complete.
- Track 0 consolidation: complete through RWP-00.81.
- RWP-00.79 owner tier/screen/trial/lifecycle decisions: recorded for repository synchronization.

## Track 0 Deliverables

- normalized cross-industry capability model;
- current-product feature/gate/limit inventory;
- capability reconciliation and gap analysis;
- unified tier and independent add-on architecture;
- limits, scope, inheritance, downgrade, and exception policy;
- cross-industry customer journey validation;
- owner approval checklist and recommended bounded implementation sequence;
- owner decision supplement for Free, paid tiers, industry recommendations, screen capacity, trials, promotions, sold-tier immutability, versioning, billing continuity, and customer migration.

The final package is `track0/consolidation/OWNER_APPROVAL_AND_IMPLEMENTATION_HANDOFF.md`.
The owner decision supplement is `track0/consolidation/RWP-00.79_OWNER_TIER_LIFECYCLE_DECISIONS.md`.

## Recorded Owner Direction

- Working ladder: Free, Operate, Coordinate, Portfolio, Enterprise.
- Free provides one complete static-image outcome on one active screen.
- Free and paid trials coexist; trial expiry falls back safely without destroying the customer’s only active public output.
- Tier determines software outcomes; active screen capacity is a separate commercial allowance.
- Industry affects recommendations and presentation, not entitlement.
- Restaurant, Café, Bar, and Food Truck normally begin at Operate; Hospitality and Entertainment normally begin at Coordinate, subject to actual operating complexity.
- A sold tier version cannot be physically deleted or silently rewritten.
- Hidden or retired tier versions continue to support existing billing and entitlement.
- Promoting a tier version changes the default for new sales only.
- Existing customers move only through an explicit optional, scheduled mandatory, or controlled administrative migration.
- Billing uses the assigned tier version, screen capacity, add-ons, promotions, contract overrides, migration effective date, and provider-confirmed state—not the public catalog.

Pricing, final numeric allowances, exact trial duration, taxes, contracts, provider commitments, and final names remain intentionally undecided.

## Exact Next Action

Complete review and merge of the RWP-00.79 owner-decision synchronization. After merge, create only bounded implementation RWPs for approved dependencies; do not resume RWP-13.06 unchanged or start Phase 14+.

## Validation Policy

Documentation validation is GitHub Actions-authoritative. Azure SQL, live Stripe, devices, hosted/browser, and integration/external-system tests remain skipped.

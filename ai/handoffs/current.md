# Vennusign Session Handoff

## Current State

- Track 0 native-industry planning: complete.
- Track 0 consolidation RWP-00.75 through RWP-00.81: complete.
- RWP-00.79 owner tier/screen/trial/lifecycle decisions: recorded on the synchronization branch.
- Product implementation: paused.
- RWP-13.06: held; do not resume unchanged.
- Phase 14 and later: paused.
- Active implementation claim: none.

## Final Package

Review:

- `track0/consolidation/OWNER_APPROVAL_AND_IMPLEMENTATION_HANDOFF.md`;
- `track0/consolidation/RWP-00.79_OWNER_TIER_LIFECYCLE_DECISIONS.md`;
- the preceding Track 0 consolidation artifacts.

## Recorded Owner Decisions

- Working tier ladder: Free, Operate, Coordinate, Portfolio, Enterprise.
- Free is one organization, one venue, one user, one active screen, and one active static image with a complete safe publish/replace/remove loop.
- Free and paid trials coexist.
- Trial configuration is independent of base tier definition and must fall back safely at expiry.
- Tier determines software outcomes; screen capacity is a separate typed allowance.
- Paid tiers include base screen capacity, with future packs, pools, or committed-volume extensions.
- Industry determines recommendation and presentation, not entitlement.
- Default first paid recommendation: Restaurant/Café/Bar/Food Truck → Operate; Hospitality/Entertainment → Coordinate, adjusted by actual complexity.
- Promotions and specials overlay subscriptions/cohorts rather than rewriting sold tiers.
- Once a tier version has ever been sold or assigned, it cannot be physically deleted or reused.
- Sold tier versions may be hidden, stopped from new sales, retired, and eventually archived while retained for billing, entitlement, audit, refunds, disputes, reporting, and legal retention.
- Promoting a new tier version makes it the default for new sales only.
- Existing customers move only through explicit migration campaigns.
- Billing uses the exact assigned tier version plus screen capacity, add-ons, promotions, contract overrides, migration effective date, and provider-confirmed state.
- The public catalog is not billing authority.
- Upgrades and downgrades require explicit effective timing, billing impact, capability/allowance changes, add-on compatibility, conflict evaluation, safe remediation, and provider/server confirmation.
- Essential correction, unpublish, active-screen safety, approved export, and recovery remain protected.

## Remaining Commercial Decisions

- final customer-facing names;
- prices, taxes, annual rules, contracts, and promotions policy;
- exact screen and other allowance values;
- exact trial durations and eligibility;
- pooling, overage, warning, and grace values;
- provider/service commitments;
- retention, export, and deletion durations.

These undecided values do not block capability, catalog-versioning, typed allowance, billing-continuity, promotion, trial, migration, or downgrade architecture planning.

## Exact Next Action

Merge the owner-decision synchronization. Then create the first bounded implementation RWP from the approved foundation sequence. Do not create a mega-RWP, resume RWP-13.06 unchanged, or start Phase 14+.

Recommended first implementation dependency remains the canonical capability registry and legacy alias foundation, followed by server capability decisions/reasons and scoped permissions. Commercial tier/version, Free/trial, screen capacity, promotion, billing continuity, and migration work should be created only with their prerequisite foundations explicit.

## Boundaries

No implementation, final pricing, billing mutation, entitlement mutation, numeric limit values, provider commitments, legal/privacy/safety policy, or Phase 14+ work is authorized by this synchronization. Azure SQL, live Stripe, devices, hosted/browser, and integration/external-system tests remain skipped.

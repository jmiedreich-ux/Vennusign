# Vennusign Session Handoff

## Current State

- Track 0 industry and product architecture: complete and closed.
- Track 1 product and architecture discussion: complete.
- Track 1 implementation: not started.
- Active implementation claim: none.
- Track 2 implementation: blocked pending Track 1 execution, validation and owner approval. Light planning may begin, but cannot be marked complete until potential changes from Track 1 and earlier tracks are evaluated.
- RWP-13.06: held; do not resume unchanged.
- Phase 14 and later: paused.

## Detailed Current Handoff

Read first:

- `ai/handoffs/2026-08-05-track-1-planning-handoff.md`

That record contains the complete approved Track 1 decisions, execution model, completion standard, demonstration boundary and owner acceptance responsibilities.

## Track 1 RWPs

1. Track 1.01 — Canonical Capability Model and Current-Code Reconciliation
2. Track 1.02 — Server Capability Decision and Reason Contract
3. Track 1.03 — Scoped Permission and Authority Model
4. Track 1.04 — Essential Core and Current Gate Replacement
5. Track 1.05 — Track Validation and Handoff

## Critical Governing Rules

- Discussion and owner agreement occur before repository/process execution work.
- Up to five fully planned and approved RWPs may execute sequentially as one batch.
- Each RWP must complete and validate its full vertical slice before the next begins.
- Every clear, bounded implementation gap must be corrected inside the same RWP and revalidated; do not defer fixable gaps into later cleanup work.
- Stop only for a new owner decision, major scope expansion, conflict, unavailable dependency, high-risk action or unresolved repeated failure.
- Conduct one owner acceptance review after the five-RWP batch.
- Light Track 2 planning may begin before or during Track 1 owner acceptance testing. It cannot be marked complete until all potential changes arising from Track 1 and earlier tracks have been evaluated and incorporated or explicitly ruled out. Track 2 implementation remains blocked until the owner approves Track 1 closure.

## Pre-Production Replacement Rule

Vennusign is a pre-production Version 1 system. Existing code, tables, SQL scripts, APIs, routes, services and tests may be changed or deleted as needed.

There is no migration requirement and no compatibility obligation. Do not use migration or legacy framing for this work.

## Approved Track 1 Foundation

- Capability IDs use `domain.resource.action`.
- Capabilities represent actual product actions/outcomes only.
- Permissions, roles, role assignments, states, allowances, add-ons, layouts and rollout controls are separate typed models.
- `publishing.*` remains a distinct capability domain.
- The server is authoritative for action decisions.
- Decision results support allowed, allowed-with-conditions, denied, unavailable and temporarily-blocked outcomes.
- Results include stable reason codes, message keys, structured parameters, resolution guidance and correlation IDs.
- Product system messages use repository-based translation catalogs and locale fallback.
- Capability, permission and scope are evaluated separately.
- Roles contain permissions; assignments are scoped and normally inherit downward.
- Track 1.04 fully replaces the current generic feature-gating architecture.
- Essential Free/core create, preview, pair, publish, confirm, replace, unpublish and recovery paths must remain usable subject to permission, state and allowance.

## Demonstration Boundary

Track 1 must not assume complete management UIs exist for roles, tiers, allowances, add-ons, rollout controls or locale administration.

Use deterministic seeds, fixtures, direct setup and test adapters for those scenarios. Use real customer-facing UI only for Track 1 surfaces actually built or affected.

Automation proves technical permutations and enforcement. The owner tests customer-visible clarity, usefulness, navigation, messaging, recovery and product intent using prepared accounts and exact test steps.

Full onboarding belongs to Track 8. Track 1 only supplies the authority and decision foundations needed by onboarding.

## Exact Next Action

1. Review the detailed handoff.
2. Revise existing Track 1 issue titles/descriptions to remove migration and legacy framing.
3. Create detailed RWP records and completeness checklists for Track 1.01 through Track 1.05.
4. Present all five for owner confirmation.
5. After confirmation, execute the sequential five-RWP batch with automatic bounded remediation.
6. Prepare and conduct the Track 1 owner acceptance review.
7. Light Track 2 planning may proceed in parallel, but it must remain provisional and cannot be marked complete until Track 1 acceptance findings and any earlier-track effects are evaluated.

## Boundaries

- This handoff does not itself authorize Track 1 implementation.
- Do not start Track 2 implementation before owner approval of Track 1 closure. Light Track 2 planning may begin, but it cannot be marked complete until potential changes from Track 1 and earlier tracks have been evaluated.
- Do not resume RWP-13.06 unchanged.
- Do not claim full onboarding or unbuilt management interfaces as Track 1 deliverables.
- Integration/external-system tests remain skipped unless separately authorized.

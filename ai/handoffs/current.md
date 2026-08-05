# Vennusign Session Handoff

## Current State

- Track 0 industry and product architecture: complete and closed.
- Track 1 product and architecture discussion: complete.
- Track 1 implementation: active; Track 1.01 through 1.04 are merged and verified, and Track 1.05 validation and owner-handoff preparation are active.
- Active implementation claim: Track 1.05 / issue #644 on `rwp/01.05-track-validation-handoff`.
- Next-track implementation: blocked pending execution, validation and owner approval of the current track. Light planning for any future track may begin, but cannot be marked complete until potential changes from the current and earlier tracks are evaluated.
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
- Light planning for any future track may begin before or during owner acceptance testing of the current track. It cannot be marked complete until all potential changes arising from the current and earlier tracks have been evaluated and incorporated or explicitly ruled out. Implementation of the next track remains blocked until the owner approves closure of the current track.

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

## Track 1.01 Completed Outcome

- `CapabilityId` enforces stable lowercase `domain.resource.action` identifiers.
- `Version1CapabilityRegistry` is the deterministic source for Version 1 product actions and outcomes across all 11 approved domains.
- Capability metadata is independent of tier, industry, provider, route, and display wording.
- `CurrentConceptReconciliation` gives every seeded generic feature key exactly one typed disposition or removal decision.
- `docs/architecture/capability-entitlement-authority.md` records the durable contract and the disposition of route/session gates, membership claims, generic feature persistence, usage strings, states, add-ons, layouts, allowances, rollout controls, and support tooling.
- Focused non-integration tests cover identifier rules, uniqueness, domain separation, prohibited identity coupling, dispositions, and registered targets.
- Exact-head Actions run 31044305223 passed affected .NET and documentation validation. PR #645 merged as `a729f4dd75468c1f69570d53f44b81dcd86a4945`; issue #640 is closed and `master` is verified. Integration/external-system tests were skipped.

## Track 1.02 Completed Outcome

- `CapabilityDecisionResult` implements allowed, allowed-with-conditions, denied, unavailable, and temporarily-blocked outcomes with stable reason, category, capability, message key, structured parameters, correlation, locale, resolution, retry, and condition fields.
- `CapabilityDecisionEngine` evaluates identity/context, rollout, entitlement, permission, add-on, allowance, resource state, and request validity independently and fails closed for unknown capabilities or incomplete inputs.
- Batch evaluation preserves capability order and correlation for navigation and dashboard projections.
- `CapabilityActionAuthorizer` requires a fresh input-provider resolution immediately before every mutation authorization; blocked calls throw the full structured decision.
- Embedded `en-US`, `fr`, and `fr-CA` product-message catalogs establish repository ownership and deterministic `fr-CA` → `fr` → `en-US` fallback.
- Focused non-integration tests cover every decision dimension, priority, conditions, failure closure, batch evaluation, mutation-time reevaluation, structured denial, and locale fallback.
- Exact-head Actions run 31044938623 passed affected API, data-access and documentation validation. PR #646 merged as `06e12569b4f4ecb196a3dbf49a4a924798626376`; issue #641 is closed and `master` is verified. Integration/external-system tests were skipped.

## Track 1.03 and 1.04 Completed Outcome

- `PermissionId`, scope and assignment types keep actor authority independent from capability availability, commercial access and product state.
- Platform, organization, venue-group, venue, resource and self scopes have explicit downward-only inheritance; future, expired and revoked assignments fail closed.
- Eight protected system roles provide deterministic customer and support permission collections; content editing and publishing remain separate authorities.
- DbUp script 053 creates and seeds permissions, protected roles and role-permission collections and adds scoped assignments, bounded support grants and support audit persistence.
- `ScopedPermissionEvaluator` and `ScopedPermissionDecisionDimensionFactory` provide exact actor/action/scope enforcement for the Track 1.02 decision engine.
- Support context requires a platform Support Operator assignment plus an explicit, reasoned, approved, time-bounded customer grant; every entry or denial is audited and successful context requires prominent indication.
- Focused non-integration tests cover scope inheritance, non-inheritance upward, protected-role boundaries, time/revocation behavior, self-scope isolation, permission decision details, support grant/role intersection, audit evidence and migration contracts.
- Track 1.03 merged through PR #647 and Track 1.04 merged through PR #648; `master` is verified at `58dcf33`.
- Track 1.04 replaced the old generic feature authority with typed commercial capability, permission, allowance, add-on, rollout, state and structured decision inputs while preserving essential core operations and recovery.

## Track 1.05 Proposed Outcome

- Combined Track 1 completeness, customer UI and player behavior have been reviewed against issue #644.
- Back Office tests pass 105/105 and display/player tests pass 136/136 locally.
- `docs/work-packages/RWP-01.05-track-validation-handoff.md` provides the scenario identities, local routes, reset guidance, offline controls, exact owner steps, expected outcomes, result recording and deferred-interface list.
- The repository does not contain shared hosted acceptance credentials. Scenario accounts must be provisioned securely in the selected test environment before owner testing.
- Exact-head GitHub Actions remains required for affected .NET, migration and documentation validation. Azure SQL and integration/external-system tests remain skipped.

## Exact Next Action

1. Publish and review the Track 1.05 PR against `master`.
2. Require exact-head affected Release build, focused non-integration tests and documentation validation.
3. Merge, close issue #644, verify `master`, and release the claim.
4. Provision the named scenario identities in the selected test environment and conduct the numbered owner tests.
5. Close Track 1 only after explicit owner approval; otherwise prepare additional Track 1 RWPs in the next scheduled chunk.
6. Keep all future-track implementation blocked. Light planning may remain provisional under the governing acceptance rule.

## Boundaries

- Only the approved Track 1.01 through Track 1.05 sequential chunk is authorized for implementation.
- Do not start implementation of the next track before owner approval of the current track's closure. Light planning for any future track may begin, but it cannot be marked complete until potential changes from the current and earlier tracks have been evaluated.
- Do not resume RWP-13.06 unchanged.
- Do not claim full onboarding or unbuilt management interfaces as Track 1 deliverables.
- Integration/external-system tests remain skipped unless separately authorized.
